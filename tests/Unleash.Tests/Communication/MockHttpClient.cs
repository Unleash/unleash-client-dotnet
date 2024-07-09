using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;
using RichardSzalay.MockHttp;

namespace Unleash.Tests.Communication
{
    internal static class MockHttpClient
    {
        internal static Tuple<MockHttpMessageHandler, UnleashApiClient> MakeMockClient(string url)
        {
            DynamicNewtonsoftJsonSerializer jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();

            var mockHttp = new MockHttpMessageHandler();

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(url)
            };

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                CustomHttpHeaders = new Dictionary<string, string>()
                {
                    { "Authorization", "*:default.some-mock-hash" }
                },
                CustomHttpHeaderProvider = null
            };

            var unleashClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders, new EventCallbackConfig());
            return new Tuple<MockHttpMessageHandler, UnleashApiClient>(mockHttp, unleashClient);

        }
    }
}