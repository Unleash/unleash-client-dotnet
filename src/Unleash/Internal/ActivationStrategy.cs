using System.Collections.Generic;

namespace Unleash.Internal
{
    internal class ActivationStrategy
    {
        public string Name { get; }
        public Dictionary<string, string> Parameters { get; }
        public List<Constraint> Constraints { get; }

        public ActivationStrategy(string name, Dictionary<string, string> parameters, List<Constraint> constraints = null)
        {
            Name = name;
            Parameters = parameters;
            Constraints = constraints ?? new List<Constraint>();
        }
    }
}