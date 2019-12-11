using System.Collections.Generic;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class RemoteAddressStrategyTests : BaseStrategyTests<RemoteAddressStrategy>
    {
        protected override RemoteAddressStrategy CreateStrategy() => new RemoteAddressStrategy();

        [Fact]
        public void IsEnabled_WhenPassedEmptyParameters_ShouldReturnFalse()
        {
            var emptyParameters = new Dictionary<string, string>();
            var context = new UnleashContext.Builder().Build();
            var result = Strategy.IsEnabled(emptyParameters, context);
            Assert.False(result);
        }

        [Theory]
        [InlineData("127.0.0.1", "127.0.0.1", true)]
        [InlineData("127.0.0.1", "127.0.0.1,10.0.0.1,196.0.0.1", true)]
        [InlineData("10.0.0.1", "127.0.0.1,10.0.0.1,196.0.0.1", true)]
        [InlineData("196.0.0.1", "127.0.0.1,10.0.0.1,196.0.0.1", true)]
        [InlineData("127.0.0.1", "127.0.0.1, 10.0.0.1, 196.0.0.1", true)]
        [InlineData("10.0.0.1", "127.0.0.1, 10.0.0.1, 196.0.0.1", true)]
        [InlineData("196.0.0.1", "127.0.0.1, 10.0.0.1, 196.0.0.1", true)]
        [InlineData("10.0.0.1", "127.0.0.1,  10.0.0.1,  196.0.0.1", true)]
        [InlineData("10.0.0.1", "127.0.0.1.10.0.0.1.196.0.0.1", false)]
        [InlineData("127.0.0.1", "10.0.0.1", false)]
        public void IsEnabled_ShouldReturnExpectedValue(string actualIp, string parameterString, bool expected)
        {
            var context = UnleashContext.New().RemoteAddress(actualIp).Build();
            var parameters = new Dictionary<string, string>
            {
                [RemoteAddressStrategy.PARAM] = parameterString
            };

            var actual = Strategy.IsEnabled(parameters, context);
            Assert.Equal(expected, actual);
        }
    }
}
