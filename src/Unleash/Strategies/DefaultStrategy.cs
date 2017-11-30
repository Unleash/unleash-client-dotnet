namespace Unleash.Strategies
{
    using System.Collections.Generic;

    public class DefaultStrategy : IStrategy
    {
        private static readonly string StrategyName = "default";

        public string Name => StrategyName;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            return true;
        }
    }
}