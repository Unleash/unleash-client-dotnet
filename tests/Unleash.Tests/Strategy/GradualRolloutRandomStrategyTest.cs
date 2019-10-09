using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class GradualRolloutRandomStrategyTest
    {
        private static GradualRolloutRandomStrategy gradualRolloutRandomStrategy;

        public GradualRolloutRandomStrategyTest()
        {
            var seed = new Random().Next();
            Console.WriteLine("GradualRolloutRandomStrategyTest running with seed: " + seed);
            gradualRolloutRandomStrategy = new GradualRolloutRandomStrategy(seed);
        }

        [Test]
        public void should_not_be_enabled_when_percentage_not_set()
        {
            var parameters = new Dictionary<string, string>();
            var enabled = gradualRolloutRandomStrategy.IsEnabled(parameters);
            enabled.Should().BeFalse();
        }

        [Test]
        public void should_not_be_enabled_when_percentage_is_not_a_not_a_number()
        {
            var parameters = new Dictionary<string, string>()
            {
                {"percentage", "foo" }
            };

            var enabled = gradualRolloutRandomStrategy.IsEnabled(parameters);
            enabled.Should().BeFalse();
        }

        [Test]
        public void should_not_be_enabled_when_percentage_is_not_a_not_a_valid_percentage_value()
        {
            var parameters = new Dictionary<string, string>()
            {
                    {"percentage", "ab" }
            };

            var enabled = gradualRolloutRandomStrategy.IsEnabled(parameters);
            enabled.Should().BeFalse();
        }

        [Test]
        public void should_never_be_enabled_when_0_percent()
        {
            var parameters = new Dictionary<string, string>()
            {
                {"percentage", "0" }
            };

            for (int i = 0; i < 1000; i++)
            {
                var enabled = gradualRolloutRandomStrategy.IsEnabled(parameters);
                enabled.Should().BeFalse();
            }
        }

        [Test]
        public void should_always_be_enabled_when_100_percent()
        {
            var parameters = new Dictionary<string, string>()
            {
                {"percentage", "100" }
            };

            for (int i = 0; i <= 100; i++)
            {
                var enabled = gradualRolloutRandomStrategy.IsEnabled(parameters);
                enabled.Should().BeTrue($"Should be enabled for p={i}");
            }
        }

        [Test]
        public void should_diverage_at_most_with_one_percent_point()
        {
            int percentage = 55;
            int min = percentage - 1;
            int max = percentage + 1;

            var parameters = new Dictionary<string, string>()
            {
                {"percentage", percentage.ToString() }
            };

            int rounds = 20000;
            int countEnabled = 0;

            for (int i = 0; i < rounds; i++)
            {
                bool enabled = gradualRolloutRandomStrategy.IsEnabled(parameters);
                if (enabled)
                {
                    countEnabled = countEnabled + 1;
                }
            }

            var measuredPercentage = Math.Round(((double) countEnabled / rounds * 100));

            (measuredPercentage >= min).Should().BeTrue();
            (measuredPercentage <= max).Should().BeTrue();
        }
    }
}