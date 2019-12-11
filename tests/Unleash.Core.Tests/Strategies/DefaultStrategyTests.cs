using System.Collections.Generic;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class DefaultStrategyTests : BaseStrategyTests<DefaultStrategy>
    {
        protected override DefaultStrategy CreateStrategy() => new DefaultStrategy();

        [Fact]
        public void IsEnabled_Always_ShouldReturnTrue()
        {
            var context = UnleashContext.New().Build();
            var parameters = new Dictionary<string, string>();

            var result = Strategy.IsEnabled(parameters, context);

            Assert.True(result);
        }
    }
}
