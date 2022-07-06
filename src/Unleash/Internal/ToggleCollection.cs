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

    public class ToggleCollection
    {
        public int Version = 1;

        private readonly Dictionary<string, FeatureToggle> togglesCache;

        private readonly Dictionary<string, Segment> segmentsCache;

        public ToggleCollection(ICollection<FeatureToggle> features = null, ICollection<Segment> segments = null)
        {
            Features = features ?? new List<FeatureToggle>(0);
            Segments = segments ?? new List<Segment>(0);

            togglesCache = new Dictionary<string, FeatureToggle>(Features.Count);
            segmentsCache = new Dictionary<string, Segment>(Segments.Count);

            foreach (var featureToggle in Features) {
                togglesCache.Add(featureToggle.Name, featureToggle);
            }

            foreach (var segment in Segments)
            {
                segmentsCache.Add(segment.Id, segment);
            }
        }

        public ICollection<FeatureToggle> Features { get; }

        public ICollection<Segment> Segments { get; }

        public FeatureToggle GetToggleByName(string name)
        {
            return togglesCache.TryGetValue(name, out var value) 
                ? value 
                : null;
        }

        public Segment GetSegmentById(string id)
        {
            return segmentsCache.TryGetValue(id, out var value)
                ? value
                : null;
        }
    }
}