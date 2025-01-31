﻿using FluentAssertions;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Tests.Mock;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_Project_Tests
    {
        private UnleashApiClient NewTestableClient(string project, MockHttpMessageHandler messageHandler)
        {
            var apiUri = new Uri("http://unleash.herokuapp.com/api/");

            var requestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceTag = "instance1",
                ConnectionId = "00000000-0000-4000-a000-000000000000",
                SdkVersion = "unleash-client-mock:0.0.0",
                CustomHttpHeaders = null,
                CustomHttpHeaderProvider = null
            };

            var httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = apiUri,
                Timeout = TimeSpan.FromSeconds(5)
            };

            return new UnleashApiClient(httpClient, requestHeaders, new EventCallbackConfig(), project);
        }

        [Test]
        public async Task FetchToggles_ForProject()
        {
            var project = "testproject";
            var messageHandler = new MockHttpMessageHandler();
            var client = NewTestableClient(project, messageHandler);

            var toggles = await client.FetchToggles("", CancellationToken.None);
            toggles.Should().NotBeNull();

            messageHandler.SentMessages.Count.Should().Be(1);
            messageHandler.SentMessages.First().RequestUri.Query.Should().Be("?project=" + project);
        }

        [Test]
        public async Task FetchToggles_WithoutProject()
        {
            string project = null;
            var messageHandler = new MockHttpMessageHandler();
            var client = NewTestableClient(project, messageHandler);

            var toggles = await client.FetchToggles("", CancellationToken.None);
            toggles.Should().NotBeNull();

            messageHandler.SentMessages.Count.Should().Be(1);
            messageHandler.SentMessages.First().RequestUri.Query.Should().Be("");
        }
    }
}
