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
    public class NumberConstraintOperator_Tests
    {
        [Test]
        public void NUM_LT_Item_Count_Of_3_Is_Less_Than_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_LT, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "3");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_LT_Item_Count_Of_5_Is_Not_Less_Than_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_LT, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "5");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_LT_Item_Count_Of_6_Is_Not_Less_Than_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_LT, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "6");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_LTE_Item_Count_Of_3_Is_Less_Than_Or_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_LTE, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "3");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_LTE_Item_Count_Of_5_Is_Less_Than_Or_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_LTE, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "5");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_LTE_Item_Count_Of_6_Is_Not_Less_Than_Or_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_LTE, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "6");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_GT_Item_Count_Of_3_Is_Not_Greater_Than_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_GT, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "3");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_GT_Item_Count_Of_5_Is_Not_Greater_Than_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_GT, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "5");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_GT_Item_Count_Of_6_Is_Greater_Than_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_GT, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "6");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_GTE_Item_Count_Of_3_Is_Not_Greater_Than_Or_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_GTE, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "3");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_GTE_Item_Count_Of_5_Is_Greater_Than_Or_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_GTE, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "5");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_GTE_Item_Count_Of_6_Is_Greater_Than_Or_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_GTE, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "6");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_EQ_Item_Count_Of_3_Is_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_EQ, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "3");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void NUM_EQ_Item_Count_Of_5_Is_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_EQ, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "5");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void NUM_EQ_Item_Count_Of_6_Is_Equal_To_Constraint_Value_5()
        {
            // Arrange
            var target = new NumberConstraintOperator();
            var constraint = new Constraint("item_count", Operator.NUM_EQ, false, false, "5");
            var context = new UnleashContext();
            context.Properties.Add("item_count", "6");
            // Act
            var result = target.Evaluate(constraint, context);

            // Assert
            result.Should().BeFalse();
        }
    }
}
