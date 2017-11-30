using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Metrics;
using Unleash.Serialization;
using Unleash.Tests.Serialization;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClientTests
    {
        private readonly IJsonSerializer jsonSerializer = new JsonNetSerializer();
        private readonly IHttpClientFactory httpClientFactory = new DefaultHttpClientFactory();

        private IUnleashApiClient CreateApiClient(UnleashApiClientRequestHeaders requestHeaders = null)
        {
            requestHeaders = requestHeaders ??  new UnleashApiClientRequestHeaders
            {
                AppName = GetType().Name,
                InstanceId = "instance1",
                CustomHttpHeaders = null
            };

            var httpClient = httpClientFactory.Create(new Uri("http://unleash.herokuapp.com/"));
            var client = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            return client;
        }

        [Test]
        public async Task FetchToggles_Success()
        {
            var api = CreateApiClient();

            var etag = ""; // first request
            var result1 = await api.FetchToggles(etag, CancellationToken.None);

            result1.HasChanged.Should().BeTrue();
            result1.ToggleCollection.Should().NotBeNull();

            Console.WriteLine(jsonSerializer.SerializeObjectToString(result1.ToggleCollection));

            //
            // NB: Could fail below if server content has changed after previous call. Just try again..
            //

            // With etag from previous response
            var result2 = await api.FetchToggles(result1.Etag, CancellationToken.None);
            result2.HasChanged.Should().BeFalse();
            result2.ToggleCollection.Should().BeNull();
            result2.Etag.Should().Be(result1.Etag);
        }

        [Test]
        public async Task RegisterClient_Success()
        {
            var client = CreateApiClient();

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

            var result = await client.RegisterClient(clientRegistration, CancellationToken.None);
            result.Should().Be(true);
        }

        [Test]
        public async Task SendMetrics_Success()
        {
            var client = CreateApiClient();

            var bucket = new MetricsBucket();
            bucket.RegisterCount("Demo123", true);
            bucket.RegisterCount("Demo123", false);
            bucket.End();

            var clientMetrics = new ClientMetrics
            {
                AppName = GetType().Name,
                InstanceId = "instance1",
                Bucket = bucket
            };

            var result = await client.SendMetrics(clientMetrics, CancellationToken.None);
            result.Should().Be(true);

            // Check result:
            // http://unleash.herokuapp.com/#/features/view/Demo123
            // http://unleash.herokuapp.com/api/admin/metrics/feature-toggles

        }
    }
}