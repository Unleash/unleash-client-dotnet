namespace Unleash.Strategies
{
    using System.Collections.Generic;

    public interface IStrategy
    {
        string Name { get; }
        bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context);
    }
}