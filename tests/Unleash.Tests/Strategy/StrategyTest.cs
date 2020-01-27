using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy
{
    public class StrategyTest
    {
        class AlwaysEnabledStrategy : IStrategy
        {
            public string Name => "enabled";

            public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context)
            {
                return true;
            }

            public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, List<Constraint> constraints)
            {
                return StrategyUtils.IsEnabled(this, parameters, context, constraints);
            }
        }

        [Test]
        public void Should_be_enabled_for_empty_constraints()
        {
            // Arrange
            var strategy = new AlwaysEnabledStrategy();
            var parameters = new Dictionary<string, string>();
            var context = new UnleashContext();
            var constraints = new List<Constraint>();

            // Act
            var enabled = strategy.IsEnabled(parameters, context, constraints);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_disabled_when_constraint_IN_is_not_satisfied()
        {
            // Arrange
            var strategy = new AlwaysEnabledStrategy();
            var parameters = new Dictionary<string, string>();
            var context = new UnleashContext
            {
                Environment = "test"
            };
            var constraints = new List<Constraint>
            {
                new Constraint("environment", Operator.IN, "prod")
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context, constraints);

            // Assert
            enabled.Should().BeFalse();
        }

        [Test]
        public void Should_be_enabled_when_constraint_IN_is_satisfied()
        {
            // Arrange
            var strategy = new AlwaysEnabledStrategy();
            var parameters = new Dictionary<string, string>();
            var context = new UnleashContext
            {
                Environment = "test"
            };
            var constraints = new List<Constraint>
            {
                new Constraint("environment", Operator.IN, "test", "prod")
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context, constraints);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_when_constraint_NOT_IN_is_satisfied()
        {
            // Arrange
            var strategy = new AlwaysEnabledStrategy();
            var parameters = new Dictionary<string, string>();
            var context = new UnleashContext
            {
                Environment = "test"
            };
            var constraints = new List<Constraint>
            {
                new Constraint("environment", Operator.NOT_IN, "prod")
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context, constraints);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_enabled_when_all_constrains_are_satisfied()
        {
            // Arrange
            var strategy = new AlwaysEnabledStrategy();
            var parameters = new Dictionary<string, string>();
            var context = new UnleashContext
            {
                Environment = "test",
                UserId = "123",
                Properties = new Dictionary<string, string>
                {
                    { "customerId", "blue" }
                }
            };
            var constraints = new List<Constraint>
            {
                new Constraint("environment", Operator.IN, "test", "prod"),
                new Constraint("userId", Operator.IN, "123"),
                new Constraint("customerId", Operator.IN, "red", "blue")
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context, constraints);

            // Assert
            enabled.Should().BeTrue();
        }

        [Test]
        public void Should_be_disabled_when_not_all_constrains_are_satisfied()
        {
            // Arrange
            var strategy = new AlwaysEnabledStrategy();
            var parameters = new Dictionary<string, string>();
            var context = new UnleashContext
            {
                Environment = "test",
                UserId = "123",
                Properties = new Dictionary<string, string>
                {
                    { "customerId", "orange" }
                }
            };
            var constraints = new List<Constraint>
            {
                new Constraint("environment", Operator.IN, "test", "prod"),
                new Constraint("userId", Operator.IN, "123"),
                new Constraint("customerId", Operator.IN, "red", "blue")
            };

            // Act
            var enabled = strategy.IsEnabled(parameters, context, constraints);

            // Assert
            enabled.Should().BeFalse();
        }
    }
}
