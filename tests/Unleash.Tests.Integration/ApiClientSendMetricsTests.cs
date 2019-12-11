using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin;
using Unleash.Metrics;
using Xunit;

namespace Unleash.Tests.Integration
{
    public class ApiClientSendMetricsTests : BaseUnleashApiClientIntegrationTests
    {
        [Fact]
        public async Task SendMetrics_WhenInvoked_CompletesSuccessfully()
        {
            var metricsBucket = new ThreadSafeMetricsBucket();
            metricsBucket.RegisterCount("Demo123", true);
            metricsBucket.RegisterCount("Demo123", false);

            var result = await Client.SendMetrics(metricsBucket, CancellationToken.None);
            Assert.True(result);

            // Check result:
            // http://unleash.herokuapp.com/#/features/view/Demo123
            // http://unleash.herokuapp.com/api/admin/metrics/feature-toggles
        }
    }
}
