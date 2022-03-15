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
    public class SemverConstraintOperator_Tests
    {
        [Test]
        public void SEMVER_GT_Patch_1_Is_Greater_Than_Patch_0()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.0", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.1");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_GT_Minor_1_Is_Greater_Than_Minor_0()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.0", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.1.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_GT_Minor_Is_Greater_Than_Patch()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.1", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.1.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_GT_Major_Is_Greater_Than_Minor()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.1.0", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "2.0.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_GT_Major_2_Is_Greater_Than_Major_1()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.0", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "2.0.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_EQ_Patch()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.2", Operator.SEMVER_EQ, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.2");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_EQ_Minor()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.2.0", Operator.SEMVER_EQ, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.2.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_EQ_Major()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "2.0.0", Operator.SEMVER_EQ, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "2.0.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_LT_Patch_1_Is_Less_Than_Patch_2()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.2", Operator.SEMVER_LT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.1");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_LT_Minor_1_Is_Less_Than_Minor_2()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.2.0", Operator.SEMVER_LT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.1.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SEMVER_LT_Major_1_Is_Less_Than_Major_2()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "2.0.0", Operator.SEMVER_LT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.0");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Beta_Is_Greater_Than_Alpha()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.2-alpha", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.2-beta");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void RC_Is_Greater_Than_Beta()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.2-beta2", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.2-rc1");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Release_Is_Greater_Than_RC()
        {
            // Arrange
            var target = new SemverConstraintOperator();
            var constraint = new Constraint("operator_semver_test", "1.0.2-rc2", Operator.SEMVER_GT, false);
            var context = new UnleashContext();
            context.Properties.Add("operator_semver_test", "1.0.2");

            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }
    }
}
