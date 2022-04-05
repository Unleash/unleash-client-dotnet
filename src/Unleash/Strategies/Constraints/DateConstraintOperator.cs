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
            DateTimeOffset? contextDate;
            if (!string.IsNullOrWhiteSpace(contextValue))
            {
                if (!DateTimeOffset.TryParse(contextValue, out var date))
                    return false;
                else
                    contextDate = date;
            }
            else
            {
                contextDate = context.CurrentTime;
                if (!contextDate.HasValue)
                    return false;
            }

            if (constraint.Inverted)
                return !Eval(constraint.Operator, constraintDate, contextDate.Value);

            return Eval(constraint.Operator, constraintDate, contextDate.Value);
        }

        private bool Eval(string @operator, DateTimeOffset constraintDate, DateTimeOffset contextDate)
        {
            if (@operator == Operator.DATE_AFTER)
                return contextDate > constraintDate;

            if (@operator == Operator.DATE_BEFORE)
                return contextDate < constraintDate;

            return false;
        }
    }
}
