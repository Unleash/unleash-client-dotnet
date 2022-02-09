﻿using System;
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

            if (!double.TryParse(contextValueString, out var contextNumber))
                return false;

            if (string.IsNullOrWhiteSpace(constraint.Value) || !double.TryParse(constraint.Value, out var constraintNumber))
                return false;

            return Eval(constraint.Operator, constraintNumber, contextNumber);
        }

        private bool Eval(Operator @operator, double constraintNumber, double contextNumber)
        {
            switch (@operator)
            {
                case Operator.NUM_EQ:
                    return contextNumber == constraintNumber;
                case Operator.NUM_GT:
                    return contextNumber > constraintNumber;
                case Operator.NUM_GTE:
                    return contextNumber >= constraintNumber;
                case Operator.NUM_LT:
                    return contextNumber < constraintNumber;
                case Operator.NUM_LTE:
                    return contextNumber <= constraintNumber;
                default:
                    return false;
            }
        }
    }
}
