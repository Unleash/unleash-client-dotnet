namespace Unleash.Strategies
{
    using System.Collections.Generic;

    public interface Strategy
    {
        string Name { get; }

        bool isEnabled(Dictionary<string, string> parameters);
        bool isEnabled(Dictionary<string, string> parameters, UnleashContext unleashContext);
    }
}