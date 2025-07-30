using FluentAssertions;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;
using Unleash.Tests.Mock;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_Features_Tests
    {
        private UnleashApiClient NewTestableClient(MockHttpMessageHandler messageHandler)
        {
            var apiUri = new Uri("http://unleash.herokuapp.com/api/");

            var jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceTag = "instance1",
                CustomHttpHeaders = null,
                CustomHttpHeaderProvider = null
            };

            var httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = apiUri,
                Timeout = TimeSpan.FromSeconds(5)
            };

            return new UnleashApiClient(httpClient, jsonSerializer, requestHeaders, new EventCallbackConfig());
        }

        [Test]
        public async Task WeakEtagIsCorrectlyAddedToRequests()
        {
            var weakETag = "W/\"1d7-RkvEPkkxAVI06R3w4JXj3w==\"";
            var messageHandler = new MockHttpMessageHandler();
            messageHandler.ETagToReturn = weakETag;
            var client = NewTestableClient(messageHandler);

            var toggles = await client.FetchToggles(weakETag, CancellationToken.None);
            toggles.HasChanged.Should().BeFalse();
        }

        [Test]
        public async Task StrongEtagIsCorrectlyAddedToRequests()
        {
            var strongETag = "\"1d7-RkvEPkkxAVI06R3w4JXj3w==\"";
            var messageHandler = new MockHttpMessageHandler();
            messageHandler.ETagToReturn = strongETag;
            var client = NewTestableClient(messageHandler);

            var toggles = await client.FetchToggles(strongETag, CancellationToken.None);
            toggles.HasChanged.Should().BeFalse();
        }

        [Test]
        public async Task NoEtagReturnsEmptyToggles()
        {
            var messageHandler = new MockHttpMessageHandler();
            var client = NewTestableClient(messageHandler);
            var toggles = await client.FetchToggles(null, CancellationToken.None);
            //cheating here because the SDK considers an empty response as no changes
            toggles.Etag.Should().BeNull();
        }
    }
}
