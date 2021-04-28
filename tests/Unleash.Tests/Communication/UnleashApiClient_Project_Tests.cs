using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Serialization;
using Unleash.Tests.Mock;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_Project_Tests
    {
        [Test]
        public async Task FetchToggles_ForProject()
        {
            var apiUri = new Uri("http://unleash.herokuapp.com/api/");
            var project = "testproject";

            var jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceTag = "instance1",
                CustomHttpHeaders = null,
                CustomHttpHeaderProvider = null
            };

            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = apiUri,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var client = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders, project);
            var toggles = await client.FetchToggles("", CancellationToken.None);
            toggles.Should().NotBeNull();
            messageHandler.SentMessages.Count.Should().Be(1);
            messageHandler.SentMessages.First().RequestUri.Query.Should().Be("?project=" + project);
        }
    }
}
