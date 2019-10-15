using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Unleash
{
    using Communication;

    public static class UnleashServiceCollectionExtensions
    {
        public static IUnleashServiceCollection WithHttpClientFactory(this IUnleashServiceCollection serviceCollection,
            Action<IHttpClientBuilder> httpClientConfigurator = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var httpClientBuilder = serviceCollection.AddHttpClient<IUnleashApiClient, UnleashApiClient>()
                .ConfigureHttpClient(
                    httpClient =>
                    {
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.ConnectionClose = false;
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true
                        };
                        httpClient.Timeout = TimeSpan.FromSeconds(5);
                    });

            httpClientConfigurator?.Invoke(httpClientBuilder);
            return serviceCollection;
        }
    }
}
