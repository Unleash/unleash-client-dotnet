using Unleash.Internal;

namespace Unleash.Caching
{
    public class ToggleCollectionCacheResult
    {
        public ToggleCollection InitialToggleCollection { get; }
        public bool IsCacheMiss { get; }
        public string InitialETag { get; }

        public static ToggleCollectionCacheResult CacheMiss { get; } = new ToggleCollectionCacheResult(null, string.Empty, true);

        public static ToggleCollectionCacheResult FromResult(ToggleCollection initialToggleCollection, string initialETag)
            => new ToggleCollectionCacheResult(initialToggleCollection, initialETag, false);

        private ToggleCollectionCacheResult(ToggleCollection initialToggleCollection, string initialETag, bool isCacheMiss)
        {
            InitialToggleCollection = initialToggleCollection;
            IsCacheMiss = isCacheMiss;
            InitialETag = initialETag ?? string.Empty;
        }
    }
}
