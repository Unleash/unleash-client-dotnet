using System.Collections.Generic;
using System.Linq;
using Unleash.Internal;

namespace Unleash.Strategies
{
    public class ConstraintUtils
    {
        public static bool Validate(List<Constraint> constraints, UnleashContext context)
        {
            if (constraints?.Count > 0)
            {
                return constraints.TrueForAll(c => ValidateConstraint(c, context));
            }
            else
            {
                return true;
            }
        }

        private static bool ValidateConstraint(Constraint constraint, UnleashContext context)
        {
            var contextValue = context.GetByName(constraint.ContextName);
            var isIn = contextValue != null && constraint.Values.Contains(contextValue.Trim());
            return (constraint.Operator == Operator.IN) == isIn;
        }
    }
}