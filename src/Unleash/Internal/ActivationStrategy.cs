using System.Collections.Generic;

namespace Unleash.Internal
{
    public class ActivationStrategy
    {
        public string Name { get; }
        public Dictionary<string, string> Parameters { get; }
        public List<Constraint> Constraints { get; }
        public List<string> Segments { get; }

        public ActivationStrategy(string name, Dictionary<string, string> parameters, List<Constraint> constraints = null, List<string> segments = null)
        {
            Name = name;
            Parameters = parameters;
            Constraints = constraints ?? new List<Constraint>();
            Segments = segments ?? new List<string>();
        }
    }
}