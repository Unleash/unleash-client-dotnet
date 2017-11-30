using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class GradualRolloutUserIdStrategyTest
    {
        private static int SEED = 892350232;
        private static long MIN = 10000000L;
        private static long MAX = 9999999999L;

        private readonly Random rand = new Random(SEED);
        private readonly List<int> percentages;

        public GradualRolloutUserIdStrategyTest()
        {
            percentages = new List<int>
            {
                1,
                2,
                5,
                10,
                25,
                50,
                90,
                99,
                100,
            };
        }

        [Test]
        public void should_have_a_name()
        {
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();
            gradualRolloutStrategy.Name.Should().Be("gradualRolloutUserId");
        }

        [Test]
        public void should_require_context()
        {
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();
            gradualRolloutStrategy.IsEnabled(new Dictionary<string, string>()).Should().BeFalse();
        }

        [Test]
        public void should_be_disabled_when_missing_user_id()
        {
            var context = UnleashContext.New().Build();
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            gradualRolloutStrategy.IsEnabled(new Dictionary<string, string>(), context).Should().BeFalse();
        }

        [Test]
        public void should_have_same_result_for_multiple_executions()
        {
            var context = UnleashContext.New().UserId("1574576830").Build();
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            var paramseters = buildParams(1, "innfinn");
            var firstRunResult = gradualRolloutStrategy.IsEnabled(paramseters, context);

            for (int i = 0; i < 10; i++)
            {
                var subsequentRunResult = gradualRolloutStrategy.IsEnabled(paramseters, context);
                firstRunResult.Should().Be(subsequentRunResult, "loginId will return same result when unchanged parameters");
            }
        }

        [Test]
        public void should_be_enabled_when_using_100percent_rollout()
        {
            var context = UnleashContext.New().UserId("1574576830").Build();
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            var paramseters = buildParams(100, "innfinn");
            var result = gradualRolloutStrategy.IsEnabled(paramseters, context);

            result.Should().BeTrue();
        }

        [Test]
        public void should_not_be_enabled_when_0percent_rollout()
        {
            var context = UnleashContext.New().UserId("1574576830").Build();
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            var paramseters = buildParams(0, "innfinn");
            var actual = gradualRolloutStrategy.IsEnabled(paramseters, context);

            actual.Should().BeFalse("should not be enabled when 0% rollout");
        }

        [Test]
        public void should_be_enabled_above_minimum_percentage()
        {
            var userId = "1574576830";
            var groupId = "";
            var minimumPercentage = StrategyUtils.GetNormalizedNumber(userId, groupId);

            var context = UnleashContext.New().UserId(userId).Build();
            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            for (var p = minimumPercentage; p <= 100; p++)
            {
                var paramseters = buildParams(p, groupId);
                var actual = gradualRolloutStrategy.IsEnabled(paramseters, context);

                actual.Should().BeTrue("should be enabled when " + p + "% rollout");
            }
        }

        [Test]
        public void should_at_most_miss_with_one_percent_when_rolling_out_to_specified_percentage()
        {
            string groupId = "group1";
            int percentage = 25;
            int rounds = 20000;
            int enabledCount = 0;

            var paramseters = buildParams(percentage, groupId);

            var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

            for (var userId = 0; userId < rounds; userId++)
            {
                var context = UnleashContext.New().UserId("user" + userId).Build();

                if (gradualRolloutStrategy.IsEnabled(paramseters, context)) {
                    enabledCount++;
                }
            }

            var actualPercentage = (enabledCount / (double) rounds) * 100.0;

            ((percentage - 1) < actualPercentage)
                .Should().BeTrue("Expected " + percentage + "%, but was " + actualPercentage + "%");

            ((percentage + 1) > actualPercentage).Should()
                .BeTrue("Expected " + percentage + "%, but was " + actualPercentage + "%");
        }

        [Test]
		[Ignore("Manual inspection")]
        public void generateReportForListOfLoginIDs()
        {
            var numberOfIDs = 200000;

            foreach (int percentage in percentages)
            {
                var numberOfEnabledUsers = checkRandomLoginIDs(numberOfIDs, percentage);
                var p = ((double) numberOfEnabledUsers / numberOfIDs) * 100.0;

                Console.WriteLine("Testing " + percentage + "% --> " + numberOfEnabledUsers + " of " + numberOfIDs + " got new feature (" + p + "%)");
            }
        }


        protected int checkRandomLoginIDs(int numberOfIDs, int percentage)
        {
            var numberOfEnabledUsers = 0;

            for (int i = 0; i < numberOfIDs; i++)
            {
                var userId = getRandomLoginId();
                var context = UnleashContext.New().UserId(userId.ToString()).Build();

                var gradualRolloutStrategy = new GradualRolloutUserIdStrategy();

                var paramseters = buildParams(percentage, "");
                var enabled = gradualRolloutStrategy.IsEnabled(paramseters, context);

                if (enabled)
                {
                    numberOfEnabledUsers++;
                }
            }

            return numberOfEnabledUsers;
        }

        private Dictionary<string, string> buildParams(int percentage, string groupId)
        {
            var paramseters = new Dictionary<string, string>();

            paramseters.Add(GradualRolloutUserIdStrategy.PercentageConst, percentage.ToString());
            paramseters.Add(GradualRolloutUserIdStrategy.GroupIdConst, groupId);

            return paramseters;
        }

        private long getRandomLoginId()
        {
            long bits, val;
            var bound = (MAX - MIN) + 1L;

            do
            {
                bits = (rand.Next() << 1) >> 1;
                val = bits % bound;
            } while (bits - val + (bound - 1L) < 0L);

            return val;
        }
    }
}