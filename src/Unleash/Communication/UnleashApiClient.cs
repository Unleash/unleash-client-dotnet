using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Metrics;
using Unleash.Serialization;

namespace Unleash.Communication
{
    internal class UnleashApiClient : IUnleashApiClient
    {
        internal UnleashApiClientRequestHeaders ClientRequestHeaders { get; }
        internal IJsonSerializer JsonSerializer { get; }

        public const string AppNameHeader = "UNLEASH-APPNAME";
        public const string InstanceIdHeader = "UNLEASH-INSTANCEID";

        private static readonly ILog Logger = LogProvider.GetLogger(typeof(UnleashApiClient));

        private readonly HttpClient httpClient;

        public UnleashApiClient(
            HttpClient httpClient,
            IJsonSerializer jsonSerializer,
            UnleashApiClientRequestHeaders clientRequestHeaders)
        {
            this.httpClient = httpClient;
            this.JsonSerializer = jsonSerializer;
            this.ClientRequestHeaders = clientRequestHeaders;
        }

        public async Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken)
        {
            const string resourceUri = "api/client/features";

            using (var request = new HttpRequestMessage(HttpMethod.Get, resourceUri))
            {
                SetRequestHeaders(request, ClientRequestHeaders);

                if (!string.IsNullOrEmpty(etag) && EntityTagHeaderValue.TryParse(etag, out var etagHeaderValue))
                {
                    request.Headers.IfNoneMatch.Add(etagHeaderValue);
                }

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.StatusCode == HttpStatusCode.NotModified
                        || (!string.IsNullOrEmpty(etag) && (response.Headers.ETag?.Tag?.Equals(etag) ?? false)))
                    {
                        return new FetchTogglesResult
                        {
                            HasChanged = false,
                            Etag = response.Headers.ETag.Tag,
                            ToggleCollection = null
                        };
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        await HandleError(response, resourceUri);

                        return new FetchTogglesResult
                        {
                            HasChanged = false,
                            Etag = null
                        };
                    }

                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var toggleCollection = JsonSerializer.Deserialize<ToggleCollection>(stream);

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
                        Etag = response.Headers.ETag?.Tag,
                        ToggleCollection = toggleCollection
                    };
                }
            }
        }

        public async Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
        {
            const string requestUri = "api/client/register";

            using (var memoryStream = new MemoryStream())
            {
                JsonSerializer.Serialize(memoryStream, registration);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return await Post(requestUri, memoryStream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<bool> SendMetrics(ThreadSafeMetricsBucket metrics, CancellationToken cancellationToken)
        {
            const string requestUri = "api/client/metrics";

            using (var memoryStream = new MemoryStream())
            {
                using (metrics.StopCollectingMetrics(out var bucket))
                {
                    JsonSerializer.Serialize(memoryStream, new ClientMetrics
                    {
                        AppName = ClientRequestHeaders.AppName,
                        InstanceId = ClientRequestHeaders.InstanceTag,
                        Bucket = bucket
                    });
                    memoryStream.Seek(0, SeekOrigin.Begin);
                }

                return await Post(requestUri, memoryStream, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<bool> Post(string resourceUri, Stream stream, CancellationToken cancellationToken)
        {
            const int bufferSize = 1024 * 4;

            using (var request = new HttpRequestMessage(HttpMethod.Post, resourceUri))
            {
                request.Content = new StreamContent(stream, bufferSize);
                request.Content.Headers.AddContentTypeJson();

                SetRequestHeaders(request, ClientRequestHeaders);

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                        return true;

                    await HandleError(response, resourceUri);

                    return false;
                }
            }
        }

        private static async Task HandleError(HttpResponseMessage response, string resourceUri)
        {
            try
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.Trace(
                    $"UNLEASH: Error {response.StatusCode} from request '{resourceUri}' in '{nameof(UnleashApiClient)}': " +
                    error);
            }
            catch (Exception)
            {
                Logger.Trace(
                    $"UNLEASH: Error {response.StatusCode} from request '{resourceUri}' in '{nameof(UnleashApiClient)}'");
            }
        }

        internal static void SetRequestHeaders(HttpRequestMessage requestMessage, UnleashApiClientRequestHeaders headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(AppNameHeader, headers.AppName);
            requestMessage.Headers.TryAddWithoutValidation(InstanceIdHeader, headers.InstanceTag);

            if (headers.CustomHttpHeaders == null)
                return;

            if (headers.CustomHttpHeaders.Count == 0)
                return;

            foreach (var header in headers.CustomHttpHeaders)
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}
