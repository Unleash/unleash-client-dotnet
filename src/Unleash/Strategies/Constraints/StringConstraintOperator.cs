using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unleash.Internal;

namespace Unleash.Strategies.Constraints
{
    public class StringConstraintOperator : IConstraintOperator
    {
        public bool Evaluate(Constraint constraint, UnleashContext context)
        {
            if (constraint.Values == null || !constraint.Values.Any())
                return false;

            var contextValue = context.GetByName(constraint.ContextName);
            if (string.IsNullOrWhiteSpace(contextValue))
                return false;

            if (constraint.Inverted)
                return !constraint.Values.Any(val => Eval(constraint.Operator, val, contextValue, constraint.CaseInsensitive));

            return constraint.Values.Any(val => Eval(constraint.Operator, val, contextValue, constraint.CaseInsensitive));
        }

        private bool Eval(string @operator, string value, string contextValue, bool caseInsensitive)
        {
            var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (@operator == Operator.STR_CONTAINS)
                return contextValue.IndexOf(value, comparison) > -1;

            if (@operator == Operator.STR_ENDS_WITH)
                return contextValue.EndsWith(value, comparison);

            if (@operator == Operator.STR_STARTS_WITH)
                return contextValue.StartsWith(value, comparison);

            return false;
        }
    }
}
