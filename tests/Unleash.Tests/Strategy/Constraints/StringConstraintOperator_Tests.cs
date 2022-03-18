﻿using FluentAssertions;
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
            var constraint = new Constraint("operator_string_test", Operator.STR_CONTAINS, false, false, null, "sentence");
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_CONTAINS_CaseInsensitive_False_Matches_With_Same_Case_On_Atleast_One_Word()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", Operator.STR_CONTAINS, false, false, null, "was", "sentence", "nothing");
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_CONTAINS_CaseInsensitive_False_Matches_With_Same_Case_On_Both_Words()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", Operator.STR_CONTAINS, false, false, null, "sentence", "should");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_CONTAINS, false, false, null, "sentence");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_CONTAINS, false, false, null, "something");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_CONTAINS, true, false, null, "sentence");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_STARTS_WITH, false, false, null, "A sentence");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_STARTS_WITH, false, false, null, "A sentence");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_STARTS_WITH, false, false, null, "something");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_STARTS_WITH, true, false, null, "a sentence");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_ENDS_WITH, false, false, null, "matched");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_ENDS_WITH, false, false, null, "matched");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_ENDS_WITH, false, false, null, "something");
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
            var constraint = new Constraint("operator_string_test", Operator.STR_ENDS_WITH, true, false, null, "matched");
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_ENDS_WITH_CaseInsensitive_False_Does_Not_Match_When_String_Is_Missing_Inverted()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", Operator.STR_ENDS_WITH, false, true, null, "something");
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();

        }

        [Test]
        public void STR_ENDS_WITH_CaseInsensitive_True_Matches_When_Case_Differs_Inverted()
        {
            // Arrange
            var target = new StringConstraintOperator();
            var constraint = new Constraint("operator_string_test", Operator.STR_ENDS_WITH, true, true, null, "matched");
            var context = new UnleashContext();
            context.Properties.Add("operator_string_test", "A sentence containing a word that should be matched");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();

        }
    }
}
