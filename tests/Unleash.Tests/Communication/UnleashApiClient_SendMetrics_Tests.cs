using NUnit.Framework;
using Unleash.Metrics;
using RichardSzalay.MockHttp;
using NUnit.Framework.Internal;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_SendMetrics_Tests
    {
        private const string BASE_URL = "http://some-mock-url/api/client";

        [Test]
        public async Task SendMetrics_Success()
        {
            var (mockHttp, client) = MockHttpClient.MakeMockClient(BASE_URL);

            mockHttp.When($"{BASE_URL}/metrics")
                .WithPartialContent("appName")
                .WithPartialContent("instanceId")
                .WithPartialContent("\"no\":0")
                .WithPartialContent("\"yes\":1")
                .Respond("application/json", "{ 'status': 'ok' }");

            var metricsBucket = new ThreadSafeMetricsBucket();

            metricsBucket.RegisterCount("someTestToggle", true);

            var result = await client.SendMetrics(metricsBucket, CancellationToken.None);
            Assert.IsTrue(result);
        }
    }
}
