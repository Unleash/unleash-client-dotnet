using System.Collections.Generic;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class GradualRolloutUserIdStrategyTests : BaseStrategyTests<GradualRolloutUserIdStrategy>
    {
        protected override GradualRolloutUserIdStrategy CreateStrategy() => new GradualRolloutUserIdStrategy();

        const string UserId = "1574576830";

        [Fact]
        public void IsEnabled_WhenPassedEmptyParameters_ShouldReturnFalse()
        {
            var emptyParameters = new Dictionary<string, string>();
            var context = new UnleashContext.Builder().Build();
            var result = Strategy.IsEnabled(emptyParameters, context);
            Assert.False(result);
        }

        [Fact]
        public void IsEnabled_WhenUserIdIsMissing_ShouldReturnFalse()
        {
            var context = UnleashContext.New().Build();
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            var result = gradualRolloutStrategy.IsEnabled(new Dictionary<string, string>(), context);
            Assert.False(result);
        }

        [Fact]
        public void IsEnabed_WhenExecutedRepeatedlyForTheSameUser_ShouldReturnTheSameResult()
        {
            var context = new UnleashContext.Builder().UserId(UserId).Build();;
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            var parameters = new Dictionary<string, string>
            {
                [GradualRolloutUserIdStrategy.PercentageConst] = "1",
                [GradualRolloutUserIdStrategy.GroupIdConst] = "innfin"
            };

            var firstRunResult = gradualRolloutStrategy.IsEnabled(parameters, context);

            for (var i = 0; i < 10; i++)
            {
                var subsequentRunResult = gradualRolloutStrategy.IsEnabled(parameters, context);
                Assert.Equal(firstRunResult, subsequentRunResult);
            }
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(100, true)]
        public void IsEnabled_WhenUsing0Or100PercentRollout_ShouldReturnFalseOrTrue(int percentage, bool expectedResult)
        {
            var context = new UnleashContext.Builder().UserId(UserId).Build();;
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            var parameters = new Dictionary<string, string>
            {
                [GradualRolloutUserIdStrategy.PercentageConst] = percentage.ToString(),
                [GradualRolloutUserIdStrategy.GroupIdConst] = "innfinn"
            };

            var result = gradualRolloutStrategy.IsEnabled(parameters, context);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsEnabled_WhenPercentageIsAboveMinimum_ShouldReturnTrue()
        {
            var groupId = string.Empty;

            var minimumPercentage = StrategyUtils.GetNormalizedNumber(UserId, groupId);

            var context = new UnleashContext.Builder().UserId(UserId).Build();;
            var parameters = new Dictionary<string, string>();
            for (var percentage = minimumPercentage; percentage <= 100; percentage++)
            {
                parameters[GradualRolloutUserIdStrategy.PercentageConst] = percentage.ToString();
                parameters[GradualRolloutUserIdStrategy.GroupIdConst] = groupId;

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

        [Fact]
        public void IsEnabled_WhenRollingOutToSpecifiedPercentage_ShouldMissByAtMostOnePercent()
        {
            var groupId = "group1";
            var percentage = 25;
            var rounds = 20000;
            var enabledCount = 0;

            var parameters = new Dictionary<string, string>
            {
                [GradualRolloutUserIdStrategy.PercentageConst] = percentage.ToString(),
                [GradualRolloutUserIdStrategy.GroupIdConst] = groupId
            };

            for (var userId = 0; userId < rounds; userId++)
            {
                var context = new UnleashContext.Builder().UserId("user" + userId).Build();

                if (Strategy.IsEnabled(parameters, context))
                {
                    enabledCount++;
                }
            }

            var actualPercentage = (enabledCount / (double) rounds) * 100.0;

            Assert.True(percentage - 1 < actualPercentage);
            Assert.True(percentage + 1 > actualPercentage);
        }
    }
}
