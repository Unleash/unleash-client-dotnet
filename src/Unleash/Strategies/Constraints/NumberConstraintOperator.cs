using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unleash.Internal;

namespace Unleash.Strategies.Constraints
{
    public class NumberConstraintOperator : IConstraintOperator
    {
        public bool Evaluate(Constraint constraint, UnleashContext context)
        {
            var contextValueString = context.GetByName(constraint.ContextName);
            if (string.IsNullOrWhiteSpace(contextValueString))
                return false;

            if (!double.TryParse(contextValueString, out var contextValue))
                return false;

            return constraint.Values?.Select(val =>
                double.TryParse(val, out var constraintVal) ? (double?)constraintVal : null
            ).Any(val => val.HasValue && Eval(constraint.Operator, val.Value, contextValue)) ?? false;
        }

        private bool Eval(Operator @operator, double constraintValue, double contextValue)
        {
            switch (@operator)
            {
                case Operator.NUM_EQ:
                    return contextValue == constraintValue;
                case Operator.NUM_GT:
                    return contextValue > constraintValue;
                case Operator.NUM_GTE:
                    return contextValue >= constraintValue;
                case Operator.NUM_LT:
                    return contextValue < constraintValue;
                case Operator.NUM_LTE:
                    return contextValue <= constraintValue;
                default:
                    return false;
            }
        }
    }
}
