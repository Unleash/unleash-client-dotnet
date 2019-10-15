using Unleash.Internal;

namespace Unleash.Caching
{
    public class ToggleCollectionCacheResult
    {
        public ToggleCollection InitialToggleCollection { get; }
        public string InitialETag { get; }

        public static ToggleCollectionCacheResult Empty { get; } = new ToggleCollectionCacheResult(null, string.Empty);

        public static ToggleCollectionCacheResult FromResult(ToggleCollection initialToggleCollection, string initialETag)
            => new ToggleCollectionCacheResult(initialToggleCollection, initialETag);

        private ToggleCollectionCacheResult(ToggleCollection initialToggleCollection, string initialETag)
        {
            InitialToggleCollection = initialToggleCollection;
            InitialETag = initialETag ?? string.Empty;
        }
    }
}
