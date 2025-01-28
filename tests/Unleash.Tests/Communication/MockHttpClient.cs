using Unleash.Communication;
using Unleash.Internal;
using RichardSzalay.MockHttp;

namespace Unleash.Tests.Communication
{
    internal static class MockHttpClient
    {
        internal static Tuple<MockHttpMessageHandler, UnleashApiClient> MakeMockClient(string url)
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(url)
            };

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                ConnectionId = "00000000-0000-4000-a000-000000000000",
                SdkVersion = "unleash-client-mock:0.0.0",
                CustomHttpHeaders = new Dictionary<string, string>()
                {
                    { "Authorization", "*:default.some-mock-hash" }
                },
                CustomHttpHeaderProvider = null
            };

            var unleashClient = new UnleashApiClient(httpClient, requestHeaders, new EventCallbackConfig());
            return new Tuple<MockHttpMessageHandler, UnleashApiClient>(mockHttp, unleashClient);

        }
    }
}