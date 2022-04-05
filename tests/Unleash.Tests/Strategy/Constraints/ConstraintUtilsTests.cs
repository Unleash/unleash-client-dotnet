using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Strategies;

namespace Unleash.Tests.Strategy.Constraints
{
    public class ConstraintUtilsTests
    {
        [Test]
        public void Invalid_Operators_Return_False()
        {
            // Setup
            var constraint = new Constraint("event_date", "INVALID_OPERATOR", false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T15:00:00.000Z");

            // Act
            var result = ConstraintUtils.Validate(new List<Constraint>() { constraint }, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Operator_Trims_Inputs()
        {
            var constraint = new Constraint("event_date", " TRIMMED  ", false, false, "2022-01-29T13:00:00.000Z");
            constraint.Operator.Should().Be("TRIMMED");
        }

        [Test]
        public void ConstraintUtils_Validate_Handles_Nulls()
        {
            // Setup
            var constraint = new Constraint("event_date", null, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T15:00:00.000Z");

            // Act
            var result = ConstraintUtils.Validate(new List<Constraint>() { constraint }, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ConstraintUtils_Validate_Handles_Empty()
        {
            // Setup
            var constraint = new Constraint("event_date", "", false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T15:00:00.000Z");

            // Act
            var result = ConstraintUtils.Validate(new List<Constraint>() { constraint }, context);

            // Assert
            result.Should().BeFalse();
        }
    }
}
