using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        private readonly UnleashEngine engine;
        private readonly string projectId;

        public UnleashApiClient(
            HttpClient httpClient, 
            IJsonSerializer jsonSerializer, 
            UnleashApiClientRequestHeaders clientRequestHeaders,
            EventCallbackConfig eventConfig,
            UnleashEngine engine,
            string projectId = null)
        {
            this.httpClient = httpClient;
            this.jsonSerializer = jsonSerializer;
            this.clientRequestHeaders = clientRequestHeaders;
            this.eventConfig = eventConfig;
            this.engine = engine;
            this.projectId = projectId;
        }

        public async Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken)
        {
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
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(FetchToggles)}': " + error);
                        eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, StatusCode = response.StatusCode, Resource = resourceUri });

                        return new FetchTogglesResult
                        {
                            HasChanged = false,
                            Etag = null,
                        };
                    }

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

                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                    if (engine != null)
                    {
                        engine.TakeState(content);
                    }

                    //var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
            }
        }

        public async Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
        {
            const string requestUri = "client/register";

            var memoryStream = new MemoryStream();
            jsonSerializer.Serialize(memoryStream, registration);

            return await Post(requestUri, memoryStream, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SendMetrics(MetricsBucket metrics, CancellationToken cancellationToken)
        {
            const string requestUri = "client/metrics";

            var memoryStream = new MemoryStream();

            jsonSerializer.Serialize(memoryStream, new ClientMetrics
            {
                AppName = clientRequestHeaders.AppName,
                InstanceId = clientRequestHeaders.InstanceTag,
                Bucket = metrics
            });

            return await Post(requestUri, memoryStream, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> Post(string resourceUri, Stream stream, CancellationToken cancellationToken)
        {
            const int bufferSize = 1024 * 4;

            using (var request = new HttpRequestMessage(HttpMethod.Post, resourceUri))
            {
                request.Content = new StreamContent(stream, bufferSize);
                request.Content.Headers.AddContentTypeJson();

                SetRequestHeaders(request, clientRequestHeaders);

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                        return true;

                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Trace($"UNLEASH: Error {response.StatusCode} from request '{resourceUri}' in '{nameof(UnleashApiClient)}': " + error);
                    eventConfig?.RaiseError(new ErrorEvent() { Resource = resourceUri, ErrorType = ErrorType.Client, StatusCode = response.StatusCode });

                    return false;
                }
            }
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
