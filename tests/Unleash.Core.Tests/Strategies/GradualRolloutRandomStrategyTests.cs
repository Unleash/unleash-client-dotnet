using System;
using System.Collections.Generic;
using Unleash.Internal;
using Unleash.Strategies;
using Xunit;

namespace Unleash.Core.Tests.Strategies
{
    public class GradualRolloutRandomStrategyTests : BaseStrategyTests<GradualRolloutRandomStrategy>
    {
        protected override GradualRolloutRandomStrategy CreateStrategy() => new GradualRolloutRandomStrategy(new UnleashRandom());

        [Fact]
        public void IsEnabled_WhenPassedEmptyParameters_ShouldReturnFalse()
        {
            var emptyParameters = new Dictionary<string, string>();
            var context = new UnleashContext.Builder().Build();
            var result = Strategy.IsEnabled(emptyParameters, context);
            Assert.False(result);
        }

        [Fact]
        public void IsEnabled_WhenPercentageNotSet_ShouldReturnFalse()
        {
            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>();

            var isEnabled = Strategy.IsEnabled(parameters, context);
            Assert.False(isEnabled);
        }

        [Fact]
        public void IsEnabled_WhenPercentageIsNotANumber_ShouldReturnFalse()
        {
            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>
            {
                ["percentage"] = "foo"
            };

            var isEnabled = Strategy.IsEnabled(parameters, context);
            Assert.False(isEnabled);
        }

        [Theory]
        [InlineData(0, false, 1000)]
        [InlineData(100, true, 1000)]
        public void IsEnabled_WhenPercentageIs0Or100_ShouldAlwaysReturnFalseOrTrue(int percentage, bool expectedResult, int iterationsToTest)
        {
            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>
            {
                ["percentage"] = percentage.ToString()
            };

            for (var x = 0; x < iterationsToTest; x++)
            {
                var isEnabled = Strategy.IsEnabled(parameters, context);
                Assert.Equal(expectedResult, isEnabled);
            }
        }

        [Theory]
        [InlineData(0, 0.0, 1.0, 0.0, 10000)]
        [InlineData(25, 0.25, 0.75, 0.1, 10000)]
        [InlineData(50, 0.5, 0.5, 0.1, 10000)]
        [InlineData(75, 0.75, 0.25, 0.1, 10000)]
        [InlineData(100, 1.0, 0.0, 0.0, 10000)]
        internal void IsEnabled_WhenPercentageIsSupplied_ShouldReturnRandomDataWithinConstraints(
            int percentage, double expectedTruePercentage, double expectedFalsePercentage,
            double expectationTolerance, int iterationsToTest)
        {
            var context = new UnleashContext.Builder().Build();
            var parameters = new Dictionary<string, string>
            {
                ["percentage"] = percentage.ToString()
            };

            int trueCount = 0;
            int falseCount = 0;
            for (var x = 0; x < iterationsToTest; x++)
            {
                var isEnabled = Strategy.IsEnabled(parameters, context);
                if (isEnabled)
                {
                    trueCount++;
                }
                else
                {
                    falseCount++;
                }
            }

            var truePercentage = (double) trueCount / iterationsToTest;
            var falsePercentage = (double) falseCount / iterationsToTest;

            Assert.True(Math.Abs(truePercentage - expectedTruePercentage) <= expectationTolerance);
            Assert.True(Math.Abs(falsePercentage - expectedFalsePercentage) <= expectationTolerance);
        }
    }
}
