using FluentAssertions;
using NUnit.Framework;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class StrategyUtilsTests
    {
        [Test]
        public void GetPercentageVariants()
        {
            StrategyUtils.GetPercentage(null).Should().Be(0);
            StrategyUtils.GetPercentage("").Should().Be(0);

            // Normal cases
            StrategyUtils.GetPercentage("0").Should().Be(0);
            StrategyUtils.GetPercentage("50").Should().Be(50);
            StrategyUtils.GetPercentage("100").Should().Be(100);

            // Whitespace
            StrategyUtils.GetPercentage(" 0 ").Should().Be(0);
            StrategyUtils.GetPercentage(" 50 ").Should().Be(50);
            StrategyUtils.GetPercentage(" 100 ").Should().Be(100);

            // Edge cases
            StrategyUtils.GetPercentage("-1").Should().Be(0);
            StrategyUtils.GetPercentage("101").Should().Be(100);

            // Min/max
            StrategyUtils.GetPercentage(int.MaxValue.ToString()).Should().Be(100);
            StrategyUtils.GetPercentage(int.MinValue.ToString()).Should().Be(0);

            // Overflow
            StrategyUtils.GetPercentage(int.MaxValue + "0").Should().Be(0);
            StrategyUtils.GetPercentage(int.MinValue + "0").Should().Be(0);
        }

        [Test]
        public void GetNormalizedNumber_Variants()
        {
            // Normal cases
            StrategyUtils.GetNormalizedNumber("user1", "group1").Should().BeInRange(0, 100);

            // Strange inputs
            StrategyUtils.GetNormalizedNumber(null, null).Should().BeInRange(0, 100);
            StrategyUtils.GetNormalizedNumber("", "").Should().BeInRange(0, 100);
            StrategyUtils.GetNormalizedNumber("#%&/(", "§~:<>&nbsp;").Should().BeInRange(0, 100);
        }

        [Test]
        public void GetNormalizedNumber_Is_Compatible_With_Java_And_Go_Implementations()
        {
            Assert.AreEqual(73, StrategyUtils.GetNormalizedNumber("123", "gr1"));
            Assert.AreEqual(25, StrategyUtils.GetNormalizedNumber("999", "groupX"));
        }
    }
}