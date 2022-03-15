using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unleash.Internal;
using Unleash.Logging;

namespace Unleash.Strategies.Constraints
{
    public class SemverConstraintOperator : IConstraintOperator
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(SemverConstraintOperator));

        public bool Evaluate(Constraint constraint, UnleashContext context)
        {
            var contextValue = context.GetByName(constraint.ContextName);
            SemanticVersion contextSemver;
            if (!SemanticVersion.TryParse(contextValue, out contextSemver))
            {
                Logger.Info("Couldn't parse version {0} from context");
                return false;
            }

            if (constraint.Values != null && constraint.Values.Any())
            {
                return constraint.Values.Where(constraintVal =>
                {
                    SemanticVersion constraintSemver;
                    return SemanticVersion.TryParse(constraintVal, out constraintSemver) && Eval(constraint.Operator, contextSemver, constraintSemver);
                }).Any();
            }
            else if (!string.IsNullOrWhiteSpace(constraint.Value))
            {
                SemanticVersion constraintSemver;
                return SemanticVersion.TryParse(constraint.Value, out constraintSemver) && Eval(constraint.Operator, contextSemver, constraintSemver);
            }

            return false;
        }

        private bool Eval(Operator @operator, SemanticVersion contextSemver, SemanticVersion constraintSemver)
        {
            switch (@operator)
            {
                case Operator.SEMVER_GT:
                    return contextSemver > constraintSemver;

                case Operator.SEMVER_EQ:
                    return contextSemver == constraintSemver;

                case Operator.SEMVER_LT:
                    return contextSemver < constraintSemver;

                default:
                    return false;
            }
        }
    }
}
