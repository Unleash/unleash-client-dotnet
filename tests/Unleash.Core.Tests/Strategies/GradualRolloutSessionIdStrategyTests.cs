using System.Collections.Generic;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class GradualRolloutSessionIdStrategyTests : BaseStrategyTests<GradualRolloutSessionIdStrategy>
    {
        protected override GradualRolloutSessionIdStrategy CreateStrategy() => new GradualRolloutSessionIdStrategy();

        private const string SessionId = "1574576830";

        [Fact]
        public void IsEnabled_WhenPassedEmptyParameters_ShouldReturnFalse()
        {
            var emptyParameters = new Dictionary<string, string>();
            var context = new UnleashContext.Builder().Build();
            var result = Strategy.IsEnabled(emptyParameters, context);
            Assert.False(result);
        }

        [Fact]
        public void IsEnabled_WhenParametersNotSet_ShouldReturnFalse()
        {
            var context = new UnleashContext.Builder().SessionId(SessionId).Build();
            var parameters = new Dictionary<string, string>();

            var isEnabled = Strategy.IsEnabled(parameters, context);
            Assert.False(isEnabled);
        }

        [Theory]
        [InlineData(100)]
        public void IsEnabled_WhenInvokedRepeatedly_ShouldReturnFalse(int iterations)
        {
            var context = new UnleashContext.Builder().SessionId(SessionId).Build();
            var parameters = new Dictionary<string, string> {
                [GradualRolloutSessionIdStrategy.Percentage] = "1",
                [GradualRolloutSessionIdStrategy.GroupId] = "innfinn"
            };

            bool firstRunResult = Strategy.IsEnabled(parameters, context);

            for (int i = 0; i < iterations; i++)
            {
                var subsequentRunResult = Strategy.IsEnabled(parameters, context);

                // loginId will return same result when unchanged parameters
                Assert.Equal(firstRunResult, subsequentRunResult);
            }
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(100, true)]
        public void IsEnabled_WhenUsing0Or100PercentRollout_ShouldReturnFalseOrTrue(int percentage, bool expectedResult)
        {
            var context = new UnleashContext.Builder().SessionId(SessionId).Build();
            var parameters = new Dictionary<string, string> {
                [GradualRolloutSessionIdStrategy.Percentage] = percentage.ToString(),
                [GradualRolloutSessionIdStrategy.GroupId] = "innfinn"
            };

            var result = Strategy.IsEnabled(parameters, context);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsEnabled_WhenPercentageIsAboveMinimum_ShouldReturnTrue()
        {
            var groupId = string.Empty;

            var minimumPercentage = StrategyUtils.GetNormalizedNumber(SessionId, groupId);

            var context = new UnleashContext.Builder().SessionId(SessionId).Build();
            var parameters = new Dictionary<string, string>();
            for (var percentage = 0; percentage <= 100; percentage++)
            {
                parameters[GradualRolloutSessionIdStrategy.Percentage] = percentage.ToString();
                parameters[GradualRolloutSessionIdStrategy.GroupId] = groupId;

                var actual = Strategy.IsEnabled(parameters, context);

                if (percentage < minimumPercentage)
                {
                    Assert.False(actual);
                }
                else
                {
                    Assert.True(actual);
                }
            }
        }
    }
}
