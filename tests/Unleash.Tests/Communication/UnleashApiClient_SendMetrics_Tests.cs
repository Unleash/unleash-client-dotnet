using FluentAssertions;
using NUnit.Framework;
using Yggdrasil;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_SendMetrics_Tests : BaseUnleashApiClientTest
    {
        [Test]
        [Ignore("Requires a valid accesstoken")]
        public async Task SendMetrics_Success()
        {
            var engine = new YggdrasilEngine();
            engine.CountFeature("Demo123", true);
            engine.CountFeature("Demo123", false);
            
            var result = await api.SendMetrics(engine.GetMetrics(), CancellationToken.None);
            result.Should().Be(true);

            // Check result:
            // http://unleash.herokuapp.com/#/features/view/Demo123
            // http://unleash.herokuapp.com/api/admin/metrics/feature-toggles    
        }
        
    }
}