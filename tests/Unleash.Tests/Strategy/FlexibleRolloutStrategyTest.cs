using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class FlexibleRolloutStrategyTest
    {
        [Test]
        public void Should_have_correct_name()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();

            // Assert
            strategy.Name.Should().Be("flexibleRollout");
        }

        [Test]
        public void Should_always_be_false()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();

            // Assert
            strategy.IsEnabled(new Dictionary<string, string>(), new UnleashContext()).Should().BeFalse();
        }

        [Test]
        public void Should_not_be_enabled_for_rollout_9_and_userId_61()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "9" },
                { "stickiness", "default" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext
            {
                UserId = "61"
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeFalse();
        }

        [Test]
        public void Should_be_enabled_for_rollout_10_and_userId_61()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "default" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext
            {
                UserId = "61"
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_for_rollout_10_and_userId_61_and_stickiness_userId()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "userId" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext
            {
                UserId = "61"
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_disabled_when_userId_is_missing()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "100" },
                { "stickiness", "userId" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext();

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeFalse();
        }

        [Test]
        public void Should_be_enabled_for_rollout_10_and_sessionId_61()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "default" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext
            {
                SessionId = "61"
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_for_rollout_10_and_sessionId_61_and_stickiness_sessionId()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "sessionId" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext
            {
                SessionId = "61"
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_for_rollout_10_and_random_value_61_and_stickiness_default()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy(() => "61");
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "default" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext();

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_for_rollout_10_and_random_value_61_and_stickiness_random()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy(() => "61");
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "random" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext();

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_not_be_enabled_for_rollout_10_and_random_value_1()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy(() => "1");
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "10" },
                { "stickiness", "default" },
                { "groupId", "Demo" }
            };
            var context = new UnleashContext();

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeFalse();
        }
    }
}
