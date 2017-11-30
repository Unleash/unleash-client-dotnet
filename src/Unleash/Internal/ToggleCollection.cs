using System.Collections.Generic;

namespace Unleash.Internal
{
    internal class ToggleCollection
    {
        public int Version = 1;

        private readonly Dictionary<string, FeatureToggle> cache;

        public ToggleCollection(ICollection<FeatureToggle> features = null)
        {
            Features = features ?? new List<FeatureToggle>(0);
            cache = new Dictionary<string, FeatureToggle>(Features.Count);

            foreach (var featureToggle in Features) {
                cache.Add(featureToggle.Name, featureToggle);
            }
        }

        public ICollection<FeatureToggle> Features { get; }

        public FeatureToggle GetToggleByName(string name)
        {
            return cache.TryGetValue(name, out var value) 
                ? value 
                : null;
        }
    }
}