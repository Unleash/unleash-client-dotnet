using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unleash.Communication;
using Unleash.Metrics;
using Unleash.Tests.Mock;
using Yggdrasil;

namespace Unleash.Tests.Communication
{
    public class UnleasRequestBodyUnitTest
    {
        private IUnleashApiClient CreateApiClient()
        {
            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                ConnectionId = "00000000-0000-4000-a000-000000000000",
            };

            var httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri("http://example.com")
            };
            var client = new UnleashApiClient(httpClient, requestHeaders, null);
            return client;
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> calls
            {
                get;
                set;
            } = new List<HttpRequestMessage>();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                calls.Add(request);

                bool isValidConnectionId = false;

                if (request.Content != null)
                {
                    string requestBody = await request.Content.ReadAsStringAsync();

                    using (JsonDocument doc = JsonDocument.Parse(requestBody))
                    {
                        if (doc.RootElement.TryGetProperty("connectionId", out JsonElement connectionIdElement))
                        {
                            if (connectionIdElement.ValueKind == JsonValueKind.String)
                            {
                                string connectionId = connectionIdElement.GetString();
                                isValidConnectionId = connectionId == "00000000-0000-4000-a000-000000000000";
                            }
                        }
                    }
                }

                return new HttpResponseMessage(isValidConnectionId ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
            }
        }

        private IUnleashApiClient api
        {
            get => TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("api") as IUnleashApiClient;
            set => TestExecutionContext.CurrentContext.CurrentTest.Properties.Set("api", value);
        }

        MockHttpMessageHandler messageHandler
        {
            get => TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("messageHandler") as MockHttpMessageHandler;
            set => TestExecutionContext.CurrentContext.CurrentTest.Properties.Set("messageHandler", value);
        }

        [SetUp]
        public void SetupTest()
        {
            messageHandler = new MockHttpMessageHandler();
        }


        [Test]
        public async Task IdentificationHttpHeaders()
        {
            api = CreateApiClient();

            var engine = new YggdrasilEngine();
            var metricsResult = await api.SendMetrics(engine.GetMetrics(), CancellationToken.None);
            var registerResult = await api.RegisterClient(new Unleash.Metrics.ClientRegistration(), CancellationToken.None);

            messageHandler.calls.Count.Should().Be(2);
            metricsResult.Should().Be(true);
            registerResult.Should().Be(true);
        }
    }
}
