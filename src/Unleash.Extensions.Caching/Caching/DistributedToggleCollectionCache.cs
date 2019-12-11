using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Caching
{
    public class DistributedToggleCollectionCache : IToggleCollectionCache<DistributedToggleCollectionCacheSettings>
    {
        public DistributedToggleCollectionCacheSettings Settings { get; }

        private readonly IDistributedCache distributedCache;
        private readonly IJsonSerializer jsonSerializer;

        public DistributedToggleCollectionCache(DistributedToggleCollectionCacheSettings settings,
            IDistributedCache distributedCache, IJsonSerializer jsonSerializer)
        {
            this.Settings = settings;
            this.distributedCache = distributedCache;
            this.jsonSerializer = jsonSerializer;
        }

        /// <inheritdoc />
        public async Task Save(ToggleCollection toggleCollection, string etag, CancellationToken cancellationToken)
        {
            if (toggleCollection == null) throw new ArgumentNullException(nameof(toggleCollection));
            if (etag == null) throw new ArgumentNullException(nameof(etag));

            cancellationToken.ThrowIfCancellationRequested();

            using (var ms = new MemoryStream())
            {
                jsonSerializer.Serialize(ms, toggleCollection);
                ms.Seek(0, SeekOrigin.Begin);

                await distributedCache.SetAsync(Settings.ToggleCollectionKeyName, ms.ToArray(), Settings.ToggleCollectionEntryOptions, cancellationToken).ConfigureAwait(false);
                await distributedCache.SetStringAsync(Settings.EtagKeyName, etag, Settings.EtagEntryOptions, CancellationToken.None).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<ToggleCollectionCacheResult> Load(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var togglesJsonBytes = await distributedCache.GetAsync(Settings.ToggleCollectionKeyName, cancellationToken).ConfigureAwait(false);
                var etag = await distributedCache.GetStringAsync(Settings.EtagKeyName, CancellationToken.None).ConfigureAwait(false);

                if (togglesJsonBytes == null || etag == null)
                {
                    return ToggleCollectionCacheResult.CacheMiss;
                }

                ToggleCollection toggleCollection;
                using (var ms = new MemoryStream(togglesJsonBytes, 0, togglesJsonBytes.Length, false))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    toggleCollection = jsonSerializer.Deserialize<ToggleCollection>(ms);
                }

                return ToggleCollectionCacheResult.FromResult(toggleCollection, etag);
            }
            catch
            {
                return ToggleCollectionCacheResult.CacheMiss;
            }
        }
    }
}
