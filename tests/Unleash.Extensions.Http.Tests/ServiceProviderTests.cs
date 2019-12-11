using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Unleash.Communication;
using Unleash.Communication.Admin;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Xunit;

namespace Unleash.Extensions.Http.Tests
{
    public class ServiceProviderTests
    {
        [Theory]
        [AutoMoqData]
        public async Task WithHttpClientFactory_RegistersNecessaryServices(
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] ToggleCollection expectedToggleCollection,
            string etag
        )
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUnleash(s =>
                {
                    s.UnleashApi = new Uri("http://localhost:4242/");
                    s.AppName = "Test";
                    s.InstanceTag = "Test";
                })
                .WithNewtonsoftJsonSerializer()
                .WithHttpClientFactory(
                    cb =>
                    {
                        cb.ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler.Object);
                    });

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var unleashApiClientFactory = serviceProvider.GetRequiredService<IUnleashApiClientFactory>();
            Assert.IsType<HttpClientFactoryApiClientFactory>(unleashApiClientFactory);

            var unleashApiClient = unleashApiClientFactory.CreateClient();
            Assert.IsType<UnleashApiClient>(unleashApiClient);

            var concreteUnleashApiClient = (UnleashApiClient) unleashApiClient;

            var clientRequestHeaders = concreteUnleashApiClient.ClientRequestHeaders;

            httpMessageHandler.SetupGetFeaturesRequestForSuccess(concreteUnleashApiClient.JsonSerializer, expectedToggleCollection, clientRequestHeaders);
            var fetchTogglesResult = await unleashApiClient.FetchToggles(etag, CancellationToken.None);

            var actualToggleCollection = fetchTogglesResult.ToggleCollection;

            AssertionUtils.AssertToggleCollectionsEquivalent(expectedToggleCollection, actualToggleCollection);
        }

        [Fact]
        public void WithAdminHttpClientFactory_RegistersNecessaryServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUnleash(s =>
                {
                    s.UnleashApi = new Uri("http://localhost:4242/");
                    s.AppName = "Test";
                    s.InstanceTag = "Test";
                })
                .WithNewtonsoftJsonSerializer()
                .WithAdminHttpClientFactory();

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var unleashAdminApiClient = serviceProvider.GetRequiredService<IUnleashAdminApiClient>();
            Assert.NotNull(unleashAdminApiClient);
        }
    }
}
