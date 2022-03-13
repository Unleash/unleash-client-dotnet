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
    public class StringConstraintOperator_Tests
    {
        [Test]
        public void STR_CONTAINS_CaseInsensitive_False_Matches_With_Same_Case()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "sentence", Operator.STR_CONTAINS, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_CONTAINS_CaseInsensitive_False_Does_Not_Match_When_Case_Differs()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "sentence", Operator.STR_CONTAINS, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A SENTENCE containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }

        [Test]
        public void STR_CONTAINS_CaseInsensitive_False_Does_Not_Match_When_String_Is_Missing()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "something", Operator.STR_CONTAINS, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }

        [Test]
        public void STR_CONTAINS_CaseInsensitive_True_Matches_When_Case_Differs()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "sentence", Operator.STR_CONTAINS, true);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A SENTENCE containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_STARTS_WITH_CaseInsensitive_False_Matches_With_Same_Case()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "A sentence", Operator.STR_STARTS_WITH, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_STARTS_WITH_CaseInsensitive_False_Does_Not_Match_When_Case_Differs()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "A sentence", Operator.STR_STARTS_WITH, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A SENTENCE containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }

        [Test]
        public void STR_STARTS_WITH_CaseInsensitive_False_Does_Not_Match_When_String_Is_Missing()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "something", Operator.STR_STARTS_WITH, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }

        [Test]
        public void STR_STARTS_WITH_CaseInsensitive_True_Matches_When_Case_Differs()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "a sentence", Operator.STR_STARTS_WITH, true);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A SENTENCE containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_ENDS_WITH_CaseInsensitive_False_Matches_With_Same_Case()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "matched", Operator.STR_ENDS_WITH, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_ENDS_WITH_CaseInsensitive_False_Does_Not_Match_When_Case_Differs()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "matched", Operator.STR_ENDS_WITH, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be MATCHED");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }

        [Test]
        public void STR_ENDS_WITH_CaseInsensitive_False_Does_Not_Match_When_String_Is_Missing()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "something", Operator.STR_ENDS_WITH, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }

        [Test]
        public void STR_ENDS_WITH_CaseInsensitive_True_Matches_When_Case_Differs()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", "matched", Operator.STR_ENDS_WITH, true);
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }
    }
}
