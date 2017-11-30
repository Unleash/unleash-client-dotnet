using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Metrics;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_SendMetrics_Tests : BaseUnleashApiClientTest
    {
        [Test]
        public async Task SendMetrics_Success()
        {
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

            var result = await api.SendMetrics(clientMetrics, CancellationToken.None);
            result.Should().Be(true);

            // Check result:
            // http://unleash.herokuapp.com/#/features/view/Demo123
            // http://unleash.herokuapp.com/api/admin/metrics/feature-toggles
        }
    }
}