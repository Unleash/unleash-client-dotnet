namespace Unleash.Strategies
{
    using System.Collections.Generic;
    using Unleash.Internal;

    public class UnknownStrategy : IStrategy
    {
        public string Name => "unknown";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            return false;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}