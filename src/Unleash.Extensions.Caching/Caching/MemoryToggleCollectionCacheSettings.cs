using Microsoft.Extensions.Caching.Memory;

namespace Unleash.Caching
{
    public class MemoryToggleCollectionCacheSettings : BaseToggleCollectionCacheSettings
    {
        public MemoryCacheEntryOptions EtagEntryOptions { get; set; } = new MemoryCacheEntryOptions();
        public MemoryCacheEntryOptions ToggleCollectionEntryOptions { get; set; } = new MemoryCacheEntryOptions();
    }
}
