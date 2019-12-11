using System.Collections.Generic;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class UnknownStrategyTests : BaseStrategyTests<UnknownStrategy>
    {
        protected override UnknownStrategy CreateStrategy() => new UnknownStrategy();

        [Fact]
        public void IsEnabled_Always_ShouldReturnFalse()
        {
            var context = UnleashContext.New().Build();
            var parameters = new Dictionary<string, string>();

            var result = Strategy.IsEnabled(parameters, context);

            Assert.False(result);
        }
    }
}
