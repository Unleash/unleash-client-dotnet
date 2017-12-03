using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_FetchToggles_Tests : BaseUnleashApiClientTest
    {
        [Test]
        public async Task Success()
        {
            var etag = ""; // first request
            var result1 = await api.FetchToggles(etag, CancellationToken.None);

            result1.HasChanged.Should().BeTrue();
            result1.ToggleCollection.Should().NotBeNull();

            result1.ToggleCollection.TraceToJson();

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
        public async Task Timer_Test()
        {
            // Warmup
            await api.FetchToggles("", CancellationToken.None);

            var stopwatch = Stopwatch.StartNew();

            var result = await api.FetchToggles("etag", CancellationToken.None);

            stopwatch.Stop();
            Console.WriteLine("Elapsed: " + stopwatch.ElapsedMilliseconds + "ms");

            result.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000);
        }
    }
}