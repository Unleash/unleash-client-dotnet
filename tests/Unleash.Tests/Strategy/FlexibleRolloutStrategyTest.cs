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

        [Test]
        public void Should_be_enabled_for_rollout_50_and_custom_stickiness_customField_388()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "50" },
                { "stickiness", "customField" },
                { "groupId", "Feature.flexible.rollout.custom.stickiness_50" }
            };
            var context = new UnleashContext()
            {
                Properties = new Dictionary<string, string>()
                {
                    { "customField", "388" }
                }
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_not_be_enabled_for_rollout_50_and_custom_stickiness_customField_402()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "50" },
                { "stickiness", "customField" },
                { "groupId", "Feature.flexible.rollout.custom.stickiness_50" }
            };
            var context = new UnleashContext
            {
                Properties = new Dictionary<string, string>
                {
                    { "customField", "402" }
                }
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeFalse();
        }

        [Test]
        public void Should_not_be_enabled_for_rollout_50_and_custom_stickiness_customField_no_value()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "50" },
                { "stickiness", "customField" },
                { "groupId", "Feature.flexible.rollout.custom.stickiness_50" }
            };
            var context = new UnleashContext();

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeFalse();
        }

        [Test]
        public void Should_be_enabled_for_rollout_100_and_custom_stickiness_customField_any_value()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "100" },
                { "stickiness", "customField" },
                { "groupId", "Feature.flexible.rollout.custom.stickiness_100" }
            };
            var context = new UnleashContext
            {
                Properties = new Dictionary<string, string>
                {
                    { "customField", "any_value" }
                }
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_without_a_context()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "100" },
                { "stickiness", "default" },
                { "groupId", "Feature.flexible.rollout.custom.stickiness_100" }
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, null);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_for_empty_userid()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var parameters = new Dictionary<string, string>
            {
                { "rollout", "100" },
                { "stickiness", "default" },
                { "groupId", "Feature.flexible.rollout.custom.stickiness_100" }
            };
            var context = new UnleashContext
            {
                UserId = string.Empty
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_only_at_most_miss_by_one_percent()
        {
            // Arrange
            var strategy = new FlexibleRolloutStrategy();
            var percentage = 25;
            var groupId = "groupId";
            var rounds = 200_000;
            var enabledCount = 0;

            // Act
            for (int i = 0; i < rounds; i++)
            {
                var parameters = new Dictionary<string, string>
                {
                    { "percentage", percentage.ToString() },
                    { "groupId", groupId }
                };
                var context = new UnleashContext
                {
                    SessionId = i.ToString()
                };

                if (strategy.IsEnabled(parameters, context))
                {
                    enabledCount++;
                }
            }

            var actualPercentage = (int)Math.Round((enabledCount / (double)rounds) * 100);
            var highMark = percentage + 1;
            var lowMark = percentage - 1;

            // Assert
            actualPercentage.Should().BeGreaterOrEqualTo(lowMark);
            actualPercentage.Should().BeLessOrEqualTo(highMark);
        }
    }
}
