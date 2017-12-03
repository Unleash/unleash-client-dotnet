using System.Collections.Generic;

namespace Unleash.Internal
{
    /// <inheritdoc />
    /// <summary>
    /// Provides synchronization control that supports multiple readers and single writer over a ToggleCollection.
    /// </summary>
    internal sealed class ThreadSafeToggleCollection : ReaderWriterLockSlimOf<ToggleCollection>
    {
    }

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