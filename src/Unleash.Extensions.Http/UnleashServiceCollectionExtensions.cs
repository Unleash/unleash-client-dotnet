using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Communication.Admin;

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
                    (sp, httpClient) =>
                    {
                        var settings = sp.GetRequiredService<UnleashSettings>();

                        httpClient.BaseAddress = settings.UnleashApi;
                        httpClient.DefaultRequestHeaders.ConnectionClose = false;

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true
                        };

                        httpClient.Timeout = TimeSpan.FromSeconds(5);
                    });

            serviceCollection.AddSingleton<IUnleashApiClientFactory, HttpClientFactoryApiClientFactory>();

            httpClientConfigurator?.Invoke(httpClientBuilder);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithAdminHttpClientFactory(this IUnleashServiceCollection serviceCollection, Action<IHttpClientBuilder> httpClientConfigurator = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var httpClientBuilder = serviceCollection.AddHttpClient<IUnleashAdminApiClient, UnleashAdminApiClient>()
                .ConfigureHttpClient(
                    (sp, httpClient) =>
                    {
                        var settings = sp.GetRequiredService<UnleashSettings>();

                        httpClient.BaseAddress = settings.UnleashApi;
                        httpClient.DefaultRequestHeaders.ConnectionClose = false;

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true
                        };
                    });

            serviceCollection.AddSingleton<IUnleashApiClientFactory, HttpClientFactoryApiClientFactory>();

            httpClientConfigurator?.Invoke(httpClientBuilder);
            return serviceCollection;
        }
    }
}
