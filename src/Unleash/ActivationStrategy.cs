using System.Collections.Generic;

namespace Unleash
{
    internal class ActivationStrategy
    {
        public ActivationStrategy(string name, Dictionary<string, string> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; }
        public Dictionary<string, string> Parameters { get; }
    }
}