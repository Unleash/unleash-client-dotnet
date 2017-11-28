using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Metrics;
using Unleash.Serialization;

namespace Unleash.Repository
{
    /// <summary>
    /// Factory for creating HttpClient used to communicate with Unleash Server api.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Called a single time during application initialization.
        /// </summary>
        HttpClient Create(Uri unleashApiUri);
    }

    /// <inheritdoc />
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        /// <summary>
        /// Default: 5 seconds
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Default: 60 seconds
        /// </summary>
        public TimeSpan ServicePointConnectionLeaseTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Default: null
        /// </summary>
        public Dictionary<string,string> CustomDefaultHttpHeaders { get; set; }

        /// <inheritdoc />
        public HttpClient Create(Uri unleashApiUri)
        {
            // Refresh DNS cache each 60 seconds
            var servicePoint = ServicePointManager.FindServicePoint(unleashApiUri);
            ConfigureServicePoint(servicePoint);

            var httpClient = new HttpClient
            {
                BaseAddress = unleashApiUri,
            };

            ConfigureHttpClient(httpClient);
            ConfigureDefaultRequestHeaders(httpClient.DefaultRequestHeaders);

            return httpClient;
        }

        protected virtual void ConfigureHttpClient(HttpClient httpClient)
        {
            httpClient.Timeout = Timeout;
        }

        protected virtual void ConfigureServicePoint(ServicePoint servicePoint)
        {
            servicePoint.ConnectionLeaseTimeout = (int)ServicePointConnectionLeaseTimeout.TotalMilliseconds;
        }

        protected virtual void ConfigureDefaultRequestHeaders(HttpRequestHeaders headers)
        {
            headers.Clear();
            headers.ConnectionClose = false;
            headers.TryAddWithoutValidation("Accept", "application/json");
            headers.TryAddWithoutValidation("Content-Type", "application/json");
            headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            if (CustomDefaultHttpHeaders != null)
            {
                foreach (var httpHeader in CustomDefaultHttpHeaders)
                {
                    headers.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
                }
            }
        }
    }

    internal class FetchTogglesResult
    {
        public bool HasChanged { get; set; }
        public string Etag { get; set; }
        public ToggleCollection ToggleCollection { get; set; }
    }

    internal class UnleashApiClientRequestHeaders
    {
        public string AppName { get; set; }   
        public string InstanceId { get; set; }   
        public string UserAgent { get; set; }   
        public Dictionary<string,string> CustomHttpHeaders { get; set; }   
    }

    internal interface IUnleashApiClient
    {
        Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken);
        Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken);
        Task<bool> SendMetrics(ClientMetrics metrics, CancellationToken cancellationToken);
    }

    internal class UnleashApiClient : IUnleashApiClient
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(UnleashApiClient));

        private readonly HttpClient httpClient;
        private readonly IJsonSerializer jsonSerializer;
        private readonly UnleashApiClientRequestHeaders clientRequestHeaders;

        public UnleashApiClient(Uri unleashApi, IHttpClientFactory httpClientFactory, IJsonSerializer jsonSerializer, UnleashApiClientRequestHeaders clientRequestHeaders)
        {
            httpClient = httpClientFactory.Create(unleashApi);

            this.jsonSerializer = jsonSerializer;
            this.clientRequestHeaders = clientRequestHeaders;
        }

        public async Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken)
        {
            const string resourceUri = "/api/client/features";

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

                        return new FetchTogglesResult
                        {
                            HasChanged = false
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

                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var toggleCollection = jsonSerializer.Deserialize<ToggleCollection>(stream);

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
            const string requestUri = "api/client/register";

            return await Post(requestUri, registration, cancellationToken);
        }

        public async Task<bool> SendMetrics(ClientMetrics metrics, CancellationToken cancellationToken)
        {
            const string requestUri = "api/client/metrics";

            return await Post(requestUri, metrics, cancellationToken);
        }

        private async Task<bool> Post(string resourceUri, object requestContent, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, resourceUri))
            {
                var jsonStream = jsonSerializer.Serialize(requestContent);
                request.Content = new StreamContent(jsonStream, 1024 * 4);
                request.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                SetRequestHeaders(request, clientRequestHeaders);

                using (var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                        return true;

                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Trace($"UNLEASH: Error {response.StatusCode} from request '{resourceUri}' in '{nameof(UnleashApiClient)}': " + error);

                    return false;
                }
            }
        }

        private static void SetRequestHeaders(HttpRequestMessage requestMessage, UnleashApiClientRequestHeaders headers)
        {
            const string appNameHeader = "UNLEASH-APPNAME";
            const string instanceIdHeader = "UNLEASH-INSTANCEID";
            const string userAgentHeader = "User-Agent";

            requestMessage.Headers.TryAddWithoutValidation(appNameHeader, headers.AppName);
            requestMessage.Headers.TryAddWithoutValidation(instanceIdHeader, headers.InstanceId);

            if (headers.UserAgent != null)
                requestMessage.Headers.TryAddWithoutValidation(userAgentHeader, headers.UserAgent);

            if (headers.CustomHttpHeaders == null)
                return;

            if (headers.CustomHttpHeaders.Count == 0)
                return;

            foreach (var header in headers.CustomHttpHeaders)
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

}