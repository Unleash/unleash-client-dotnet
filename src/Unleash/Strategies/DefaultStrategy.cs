namespace Unleash.Strategies
{
    using System.Collections.Generic;
    using Unleash.Internal;

    public class DefaultStrategy : IStrategy
    {
        private static readonly string StrategyName = "default";

        public string Name => StrategyName;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            return true;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}