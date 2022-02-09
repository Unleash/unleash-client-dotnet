using System;
using System.Collections.Generic;
using System.Text;
using Unleash.Internal;

namespace Unleash.Strategies.Constraints
{
    public class DateConstraintOperator : IConstraintOperator
    {
        public bool Evaluate(Constraint constraint, UnleashContext context)
        {
            if (string.IsNullOrWhiteSpace(constraint.Value) || !DateTimeOffset.TryParse(constraint.Value, out var constraintDate))
                return false;

            var contextValue = context.GetByName(constraint.ContextName);
            if (string.IsNullOrWhiteSpace(contextValue) || !DateTimeOffset.TryParse(contextValue, out var contextDate))
                return false;

            return Eval(constraint.Operator, constraintDate, contextDate);
        }

        private bool Eval(Operator @operator, DateTimeOffset constraintDate, DateTimeOffset contextDate)
        {
            if (@operator == Operator.DATE_AFTER)
                return contextDate > constraintDate;

            if (@operator == Operator.DATE_BEFORE)
                return contextDate < constraintDate;

            return false;
        }
    }
}
