using Microsoft.Extensions.Caching.Distributed;

namespace Unleash.Caching
{
    public class DistributedToggleCollectionCacheSettings : BaseToggleCollectionCacheSettings
    {
        public DistributedCacheEntryOptions ToggleCollectionEntryOptions { get; set; } = new DistributedCacheEntryOptions();
        public DistributedCacheEntryOptions EtagEntryOptions { get; set; } = new DistributedCacheEntryOptions();
    }
}
