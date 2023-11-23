﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Events;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Metrics;
using Unleash.Serialization;

namespace Unleash.Communication
{
    internal class UnleashApiClient : IUnleashApiClient
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(UnleashApiClient));

        private readonly HttpClient httpClient;
        private readonly IJsonSerializer jsonSerializer;
        private readonly UnleashApiClientRequestHeaders clientRequestHeaders;
        private readonly EventCallbackConfig eventConfig;
        private readonly string projectId;
        private int featureRequestsToSkip = 0;
        private int featureRequestsSkipped = 0;
        private int metricsRequestsToSkip = 0;
        private int metricsRequestsSkipped = 0;
        private readonly int[] backoffResponses = 
            new int[]
                {
                    429,
                    500,
                    502,
                    503,
                    504                 
                };
        private readonly int[] configurationErrorResponses =
            new int[]
                {
                    401,
                    403,
                    404,
                };
        public UnleashApiClient(
            HttpClient httpClient, 
            IJsonSerializer jsonSerializer, 
            UnleashApiClientRequestHeaders clientRequestHeaders,
            EventCallbackConfig eventConfig,
            string projectId = null)
        {
            this.httpClient = httpClient;
            this.jsonSerializer = jsonSerializer;
            this.clientRequestHeaders = clientRequestHeaders;
            this.eventConfig = eventConfig;
            this.projectId = projectId;
        }

        public async Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken)
        {
            if (featureRequestsToSkip > featureRequestsSkipped)
            {
                featureRequestsSkipped++;
                return new FetchTogglesResult
                {
                    HasChanged = false,
                    Etag = null,
                };
            }

            featureRequestsSkipped = 0;

            string resourceUri = "client/features";
            if (!string.IsNullOrWhiteSpace(this.projectId))
                resourceUri += "?project=" + this.projectId;

            using (var request = new HttpRequestMessage(HttpMethod.Get, resourceUri))
            {
                SetRequestHeaders(request, clientRequestHeaders);

                if (EntityTagHeaderValue.TryParse(etag, out var etagHeaderValue))
                    request.Headers.IfNoneMatch.Add(etagHeaderValue);

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotModified)
                    {
                        return await HandleErrorResponse(response, resourceUri);
                    }

                    return await HandleSuccessResponse(response, etag);
                }
            }
        }

        private async Task<FetchTogglesResult> HandleErrorResponse(HttpResponseMessage response, string resourceUri)
        {
            if (backoffResponses.Contains((int)response.StatusCode))
            {
                Backoff(response);
            }

            if (configurationErrorResponses.Contains((int)response.StatusCode))
            {
                ConfigurationError(response, resourceUri);
            }

            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(FetchToggles)}': " + error);
            eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, StatusCode = response.StatusCode, Resource = resourceUri });

            return new FetchTogglesResult
            {
                HasChanged = false,
                Etag = null,
            };
        }
        private void Backoff(HttpResponseMessage response)
        {
            featureRequestsToSkip = Math.Min(10, featureRequestsToSkip + 1);
            Logger.Warn($"UNLEASH: Backing off due to {response.StatusCode} from server in '{nameof(FetchToggles)}'.");
        }

        private void ConfigurationError(HttpResponseMessage response, string requestUri)
        {
            featureRequestsToSkip = 10;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.Error($"UNLEASH: Error when fetching toggles, {requestUri} responded NOT_FOUND (404) which means your API url most likely needs correction.'.");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                Logger.Error($"UNLEASH: Error when fetching toggles, {requestUri} responded FORBIDDEN (403) which means your API token is not valid.");
            }
            else
            {
                Logger.Error($"UNLEASH: Configuration error due to {response.StatusCode} from server in '{nameof(FetchToggles)}'.");
            }
        }

        private async Task<FetchTogglesResult> HandleSuccessResponse(HttpResponseMessage response, string etag)
        {
            featureRequestsToSkip = Math.Max(0, featureRequestsToSkip - 1);

            var newEtag = response.Headers.ETag?.Tag;
            if (newEtag == etag)
            { 
                return new FetchTogglesResult
                {
                    HasChanged = false,
                    Etag = newEtag,
                    ToggleCollection = null,
                };
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var toggleCollection = jsonSerializer.Deserialize<ToggleCollection>(stream);

            if (toggleCollection == null)
            {
                return new FetchTogglesResult
                {
                    HasChanged = false
                };
            }

            // Success
            return new FetchTogglesResult
            {
                HasChanged = true,
                Etag = newEtag,
                ToggleCollection = toggleCollection
            };
        }

        public async Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
        {
            const string requestUri = "client/register";

            var memoryStream = new MemoryStream();
            jsonSerializer.Serialize(memoryStream, registration);

            const int bufferSize = 1024 * 4;

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                request.Content = new StreamContent(memoryStream, bufferSize);
                request.Content.Headers.AddContentTypeJson();

                SetRequestHeaders(request, clientRequestHeaders);

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                        return true;

                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Trace($"UNLEASH: Error {response.StatusCode} from request '{requestUri}' in '{nameof(UnleashApiClient)}': " + error);
                    eventConfig?.RaiseError(new ErrorEvent() { Resource = requestUri, ErrorType = ErrorType.Client, StatusCode = response.StatusCode });

                    return false;
                }
            }
        }

        public async Task<bool> SendMetrics(ThreadSafeMetricsBucket metrics, CancellationToken cancellationToken)
        {
            if (metricsRequestsToSkip > metricsRequestsSkipped)
            {
                metricsRequestsSkipped++;
                return false;
            }

            metricsRequestsSkipped = 0;

            const string requestUri = "client/metrics";

            var memoryStream = new MemoryStream();

            using (metrics.StopCollectingMetrics(out var bucket))
            {
                jsonSerializer.Serialize(memoryStream, new ClientMetrics
                {
                    AppName = clientRequestHeaders.AppName,
                    InstanceId = clientRequestHeaders.InstanceTag,
                    Bucket = bucket
                });
            }

            const int bufferSize = 1024 * 4;

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                request.Content = new StreamContent(memoryStream, bufferSize);
                request.Content.Headers.AddContentTypeJson();

                SetRequestHeaders(request, clientRequestHeaders);

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotModified)
                    {
                        HandleMetricsSuccessResponse(response);
                        return true;
                    }

                    await HandleMetricsErrorResponse(response, requestUri);
                    return false;
                }
            }
        }

        private async Task HandleMetricsErrorResponse(HttpResponseMessage response, string requestUri)
        {
            if (backoffResponses.Contains((int)response.StatusCode))
            {
                metricsRequestsToSkip = Math.Min(10, metricsRequestsToSkip + 1);
            }

            if (configurationErrorResponses.Contains((int)response.StatusCode))
            {
                metricsRequestsToSkip = 10;
            }

            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Logger.Trace($"UNLEASH: Error {response.StatusCode} from request '{requestUri}' in '{nameof(UnleashApiClient)}': " + error);
            eventConfig?.RaiseError(new ErrorEvent() { Resource = requestUri, ErrorType = ErrorType.Client, StatusCode = response.StatusCode });
        }

        private void HandleMetricsSuccessResponse(HttpResponseMessage response)
        {
            metricsRequestsToSkip = Math.Max(0, metricsRequestsToSkip - 1);
        }

        private static void SetRequestHeaders(HttpRequestMessage requestMessage, UnleashApiClientRequestHeaders headers)
        {
            const string appNameHeader = "UNLEASH-APPNAME";
            const string userAgentHeader = "User-Agent";
            const string instanceIdHeader = "UNLEASH-INSTANCEID";

            const string supportedSpecVersionHeader = "Unleash-Client-Spec";

            requestMessage.Headers.TryAddWithoutValidation(appNameHeader, headers.AppName);
            requestMessage.Headers.TryAddWithoutValidation(userAgentHeader, headers.AppName);
            requestMessage.Headers.TryAddWithoutValidation(instanceIdHeader, headers.InstanceTag);
            requestMessage.Headers.TryAddWithoutValidation(supportedSpecVersionHeader, headers.SupportedSpecVersion);

            SetCustomHeaders(requestMessage, headers.CustomHttpHeaders);
            SetCustomHeaders(requestMessage, headers.CustomHttpHeaderProvider?.CustomHeaders);
        }

        private static void SetCustomHeaders(HttpRequestMessage requestMessage, Dictionary<string, string> headers)
        {
            if (headers == null)
                return;

            if (headers.Count == 0)
                return;

            foreach (var header in headers)
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}
