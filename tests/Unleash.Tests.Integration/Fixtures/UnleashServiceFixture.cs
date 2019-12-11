using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Unleash.Communication.Admin;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Tests.Integration.Fixtures
{
    public class UnleashServiceFixture : IDisposable
    {
        public UnleashSettings Settings { get; }
        public IUnleashServices UnleashServices { get; }
        public IUnleash Unleash { get; }
        public IUnleashContextProvider ContextProvider { get; }
        public IJsonSerializer JsonSerializer { get; }
        public IUnleashAdminApiClient AdminApiClient { get; }

        public UnleashServiceFixture()
        {
            Settings = new UnleashSettings
            {
                UnleashApi = new Uri("http://localhost:4242/"),
                AppName = "IntegrationTest",
                InstanceTag = "Test"
            };

            UnleashServices = new DefaultUnleashServices(Settings);
            ContextProvider = new DefaultUnleashContextProvider();
            Unleash = new Unleash(Settings, UnleashServices, ContextProvider);

            UnleashServices?.FeatureToggleLoadComplete(false, CancellationToken.None).Wait();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:4242/admin/")
            };

            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            JsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);
            AdminApiClient = new UnleashAdminApiClient(httpClient, JsonSerializer);
        }

        public void Dispose()
        {
            UnleashServices.Dispose();
        }
    }
}
