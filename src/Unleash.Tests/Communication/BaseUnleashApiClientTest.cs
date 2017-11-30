using System;
using System.Threading;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Serialization;

namespace Unleash.Tests.Communication
{
    public abstract class BaseUnleashApiClientTest
    {
        private static readonly Lazy<IUnleashApiClient> ApiClient =
            new Lazy<IUnleashApiClient>(
                CreateApiClient,
                LazyThreadSafetyMode.PublicationOnly);

        private static IUnleashApiClient Client => ApiClient.Value;

        private static IUnleashApiClient CreateApiClient()
        {
            var apiUri = new Uri("http://unleash.herokuapp.com/");

            var jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();

            var httpClientFactory = new DefaultHttpClientFactory();

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceId = "instance1",
                CustomHttpHeaders = null
            };

            var httpClient = httpClientFactory.Create(apiUri);
            var client = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            return client;
        }

        internal IUnleashApiClient api;

        [SetUp]
        public void SetupTest()
        {
            api = Client;
        }
    }
}