using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Strategies.Constraints;

namespace Unleash.Tests.Strategy.Constraints
{
    public class DateConstraintOperator_Tests
    {
        [Test]
        public void DATE_AFTER_2_Hours_Later_Is_After()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_AFTER, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T15:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void DATE_AFTER_Same_Time_Is_Not_After()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_AFTER, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T13:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void DATE_AFTER_2_Hours_Before_Is_Not_After()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_AFTER, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T11:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void DATE_BEFORE_2_Hours_Earlier_Is_Before()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_BEFORE, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T11:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void DATE_AFTER_Same_Time_Is_Not_Before()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_BEFORE, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T13:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void DATE_AFTER_2_Hours_Later_Is_Not_Before()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_BEFORE, false, false, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T15:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void DATE_BEFORE_2_Hours_Earlier_Is_Before_Inverted()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_BEFORE, false, true, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T11:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void DATE_AFTER_Same_Time_Is_Not_Before_Inverted()
        {

            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("event_date", Operator.DATE_BEFORE, false, true, "2022-01-29T13:00:00.000Z");
            var context = new UnleashContext();
            context.Properties.Add("event_date", "2022-01-29T13:00:00.000Z");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void DATE_AFTER_CurrentTime()
        {
            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("currentTime", Operator.DATE_AFTER, false, false, DateTime.UtcNow.AddDays(-30).ToString("O"));
            var context = new UnleashContext.Builder().Now().Build();

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void DATE_AFTER_CurrentTime_Not_Set_Uses_UtcNow()
        {
            // Arrange
            var target = new DateConstraintOperator();
            var constraint = new Constraint("currentTime", Operator.DATE_AFTER, false, false, DateTime.UtcNow.AddDays(-30).ToString("O"));
            var context = new UnleashContext();

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }
    }
}
