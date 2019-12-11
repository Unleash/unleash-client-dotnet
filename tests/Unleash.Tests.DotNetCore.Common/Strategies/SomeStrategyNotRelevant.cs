using System.Collections.Generic;
using Unleash.Strategies;

namespace Unleash.Tests.DotNetCore.Strategies
{
    public class SomeStrategyNotRelevant : IStrategy
    {
        public string Name { get; } = "NotRelevant";
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context) => true;
    }
}
