using System;
using Unleash.Communication;
using Unleash.Communication.Admin;
using Unleash.Serialization;

namespace Unleash.Tests.Integration
{
    public abstract class BaseUnleashApiClientIntegrationTests
    {
        protected IUnleashApiClient Client { get; }
        protected IUnleashAdminApiClient AdminClient { get; }

        protected BaseUnleashApiClientIntegrationTests()
        {
            var apiUri = new Uri("http://localhost:4242/");

            var jsonSerializer = new NewtonsoftJsonSerializer(new NewtonsoftJsonSerializerSettings());

            var httpClientFactory = new DefaultHttpClientFactory();

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceTag = "instance1",
                CustomHttpHeaders = null
            };

            var httpClient = httpClientFactory.Create(apiUri);
            Client = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            AdminClient = new UnleashAdminApiClient(httpClient, jsonSerializer);
        }
    }
}
