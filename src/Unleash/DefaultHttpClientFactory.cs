using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Unleash
{
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

        public virtual HttpClient NewHttpClient()
        {
            return new HttpClient();
        }

        public HttpClient Create(Uri unleashApiUri)
        {
            // Refresh DNS cache each 60 seconds
            var servicePoint = ServicePointManager.FindServicePoint(unleashApiUri);
            ConfigureServicePoint(servicePoint);

            var httpClient = NewHttpClient();
            httpClient.BaseAddress = unleashApiUri;
            httpClient.Timeout = Timeout;

            ConfigureHttpClient(httpClient);
            ConfigureDefaultRequestHeaders(httpClient.DefaultRequestHeaders);

            return httpClient;
        }

        protected virtual void ConfigureHttpClient(HttpClient httpClient)
        {
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
}