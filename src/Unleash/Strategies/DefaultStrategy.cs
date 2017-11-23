namespace Unleash.Strategies
{
    using System.Collections.Generic;

    public class DefaultStrategy : IStrategy
    {
        private static string STRATEGY_NAME = "default";

        public string Name => STRATEGY_NAME;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            return true;
        }
    }
}