using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class GradualRolloutSessionIdStrategyTest
    {
        private static int SEED = 892350151;
        private static long MIN = 10000000L;
        private static long MAX = 9999999999L;

        private readonly Random rand = new Random(SEED);
        private readonly List<int> percentages;

        public GradualRolloutSessionIdStrategyTest()
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
                100
            };
        }

        [Test]
        public void should_have_a_name()
        {
            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();
            gradualRolloutStrategy.Name.Should().Be("gradualRolloutSessionId");
        }

        [Test]
        public void should_require_context()
        {
            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();
            gradualRolloutStrategy.IsEnabled(new Dictionary<string, string>()).Should().BeFalse();
        }

        [Test]
        public void should_be_disabled_when_missing_user_id()
        {
            var context = UnleashContext.New().Build();
            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();

            gradualRolloutStrategy.IsEnabled(new Dictionary<string, string>(), context).Should().BeFalse();
        }

        [Test]
        public void should_have_same_result_for_multiple_executions()
        {
            var context = UnleashContext.New().SessionId("1574576830").Build();
            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();

            var parameters = buildParams(1, "innfinn");
            bool firstRunResult = gradualRolloutStrategy.IsEnabled(parameters, context);

            for (int i = 0; i < 10; i++)
            {
                var subsequentRunResult = gradualRolloutStrategy.IsEnabled(parameters, context);
                firstRunResult.Should().Be(subsequentRunResult, "loginId will return same result when unchanged parameters");
            }
        }

        [Test]
        public void should_be_enabled_when_using_100percent_rollout()
        {
            var context = UnleashContext.New().SessionId("1574576830").Build();
            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();

            var parameters = buildParams(100, "innfinn");
            var result = gradualRolloutStrategy.IsEnabled(parameters, context);

            result.Should().BeTrue();
        }


        [Test]
        public void should_not_be_enabled_when_0percent_rollout()
        {
            var context = UnleashContext.New().SessionId("1574576830").Build();
            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();

            var parameters = buildParams(0, "innfinn");
            var actual = gradualRolloutStrategy.IsEnabled(parameters, context);

            actual.Should().BeFalse("should not be enabled when 0% rollout");
        }

        [Test]
        public void should_be_enabled_above_minimum_percentage()
        {
            string sessionId = "1574576830";
            string groupId = "";
            int minimumPercentage = StrategyUtils.GetNormalizedNumber(sessionId, groupId);

            var context = UnleashContext.New().SessionId(sessionId).Build();

            var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();

            for (int p = minimumPercentage; p <= 100; p++)
            {
                var parameters = buildParams(p, groupId);
                var actual = gradualRolloutStrategy.IsEnabled(parameters, context);
                actual.Should().BeTrue("should be enabled when " + p + "% rollout");
            }
        }
        
        [Test]
        [Ignore("Intended for manual execution")]
        public void generateReportForListOfLoginIDs()
        {
            var numberOfIDs = 200000;

            foreach (int percentage in percentages) {
                var numberOfEnabledUsers = checkRandomLoginIDs(numberOfIDs, percentage);
                var p = (numberOfEnabledUsers / (double) numberOfIDs) * 100.0;
                Console.WriteLine($"Testing {percentage}% --> {numberOfEnabledUsers} of {numberOfIDs} got new feature ({p}%)");
            }
        }

        protected int checkRandomLoginIDs(int numberOfIDs, int percentage)
        {
            int numberOfEnabledUsers = 0;
            for (int i = 0; i < numberOfIDs; i++)
            {
                var sessionId = getRandomLoginId();
                var context = UnleashContext.New().SessionId(sessionId.ToString()).Build();

                var gradualRolloutStrategy = new GradualRolloutSessionIdStrategy();

                var parameters = buildParams(percentage, "");
                bool enabled = gradualRolloutStrategy.IsEnabled(parameters, context);
                if (enabled)
                {
                    numberOfEnabledUsers++;
                }
            }
            return numberOfEnabledUsers;
        }

        private Dictionary<string, string> buildParams(int percentage, string groupId)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add(GradualRolloutSessionIdStrategy.Percentage, percentage.ToString());
            parameters.Add(GradualRolloutSessionIdStrategy.GroupId, groupId);

            return parameters;
        }


        private long getRandomLoginId()
        {
            long bits, val;
            long bound = (MAX - MIN) + 1L;
            do
            {
                bits = (rand.Next() << 1) >> 1;
                val = bits % bound;
            } while (bits - val + (bound - 1L) < 0L);
            return val;
        }
    }
}