namespace Unleash.Strategies
{
    using System.Collections.Generic;

    public class UnknownStrategy : IStrategy
    {
        public string Name => "unknown";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            return false;
        }
    }
}