using System.Collections.Generic;

namespace Unleash.Tests.Example
{
    class CustomStrategy : Strategies.IStrategy
    {
        public string Name => "custom";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            return false;
        }
    }
}