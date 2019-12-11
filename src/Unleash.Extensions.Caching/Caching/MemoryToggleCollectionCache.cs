using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Unleash.Internal;

namespace Unleash.Caching
{
    public class MemoryToggleCollectionCache : IToggleCollectionCache<MemoryToggleCollectionCacheSettings>
    {
        public MemoryToggleCollectionCacheSettings Settings { get; }
        private IMemoryCache MemoryCache { get; }

        public MemoryToggleCollectionCache(MemoryToggleCollectionCacheSettings settings, IMemoryCache memoryCache)
        {
            this.Settings = settings;
            this.MemoryCache = memoryCache;
        }

        public Task<ToggleCollectionCacheResult> Load(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (MemoryCache.TryGetValue(Settings.ToggleCollectionKeyName, out ToggleCollection toggles)
                && MemoryCache.TryGetValue(Settings.EtagKeyName, out string etag))
            {
                return Task.FromResult(ToggleCollectionCacheResult.FromResult(toggles, etag));
            }

            return Task.FromResult(ToggleCollectionCacheResult.CacheMiss);
        }

        /// <inheritdoc />
        public Task Save(ToggleCollection toggleCollection, string etag, CancellationToken cancellationToken)
        {
            if (toggleCollection == null) throw new ArgumentNullException(nameof(toggleCollection));
            if (etag == null) throw new ArgumentNullException(nameof(etag));

            cancellationToken.ThrowIfCancellationRequested();

            MemoryCache.Set(Settings.ToggleCollectionKeyName, toggleCollection, Settings.ToggleCollectionEntryOptions);
            MemoryCache.Set(Settings.EtagKeyName, etag, Settings.EtagEntryOptions);

            return Task.CompletedTask;
        }
    }
}
