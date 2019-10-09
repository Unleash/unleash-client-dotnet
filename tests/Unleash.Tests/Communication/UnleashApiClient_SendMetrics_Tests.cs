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
            var metricsBucket = new ThreadSafeMetricsBucket();
            metricsBucket.RegisterCount("Demo123", true);
            metricsBucket.RegisterCount("Demo123", false);
            
            var result = await api.SendMetrics(metricsBucket, CancellationToken.None);
            result.Should().Be(true);

            // Check result:
            // http://unleash.herokuapp.com/#/features/view/Demo123
            // http://unleash.herokuapp.com/api/admin/metrics/feature-toggles    
        }
        
    }
}