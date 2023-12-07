using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Metrics;
using Unleash.Serialization;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_RegisterClient_Tests : BaseUnleashApiClientTest
    {
        [Test]
        [Ignore("Requires a valid accesstoken")]
        public async Task RegisterClient_Success()
        {
            var clientRegistration = new ClientRegistration()
            {
                AppName = GetType().Name,
                InstanceId = "instance1",
                Interval = 1000,
                SdkVersion = "sdk101",
                Started = DateTimeOffset.UtcNow,
                Strategies = new List<string>
                {
                    "abc"
                }
            };

            var result = await api.RegisterClient(clientRegistration, CancellationToken.None);
            result.Should().Be(true);
        }

        [Test]
        public async Task Headers()
        {
            var jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();
            var messageHandler = new InvokingMessageHandler((message) => {
                Assert.AreEqual(true, message.Content.Headers.Contains("Content-Length"), "Content-Length header is not set");
                Assert.AreEqual("183", message.Content.Headers.GetValues("Content-Length").First(), "Content-Length header value mismatch");
            });
            var client = new HttpClient(messageHandler);
            client.BaseAddress = new Uri("http://localhost:8080/api/");
            var apiClient = new UnleashApiClient(client, jsonSerializer, new UnleashApiClientRequestHeaders(), new EventCallbackConfig());
            var clientRegistration = new ClientRegistration()
            {
                AppName = GetType().Name,
                InstanceId = "instance1",
                Interval = 1000,
                SdkVersion = "sdk101",
                Started = DateTimeOffset.UtcNow,
                Strategies = new List<string>
                {
                    "abc"
                }
            };

            var result = await apiClient.RegisterClient(clientRegistration, CancellationToken.None);
            result.Should().Be(true);
        }
    }

    internal class InvokingMessageHandler : HttpMessageHandler
    {
        private readonly Action<HttpRequestMessage> action;

        internal InvokingMessageHandler(Action<HttpRequestMessage> action)
        {
            this.action = action;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            action(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}