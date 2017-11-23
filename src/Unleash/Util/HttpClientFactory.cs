using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Unleash.Util
{
    public class HttpClientFactory
    {
        private readonly UnleashConfig config;

        public HttpClientFactory(UnleashConfig config)
        {
            this.config = config;
        }

        public HttpClient Create()
        {
            var servicePoint = ServicePointManager.FindServicePoint(config.UnleashApi);
            servicePoint.ConnectionLeaseTimeout = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5),
                BaseAddress = config.UnleashApi,
            };

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            return httpClient;
        }
    }
}