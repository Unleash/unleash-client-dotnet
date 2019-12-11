using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Memory;
using Unleash.Caching;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Xunit;

namespace Unleash.Extensions.Caching.Tests
{
    public class MemoryToggleCollectionCacheTests
    {
        [Theory]
        [AutoMoqData]
        public async Task Save_WhenToggleCollectionIsNull_ThrowsArgumentNullException(
            MemoryToggleCollectionCache cache,
            string etag
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.Save(null, etag, CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Save_WhenEtagIsNull_ThrowsArgumentNullException(
            MemoryToggleCollectionCache cache,
            ToggleCollection toggleCollection
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.Save(toggleCollection, null, CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Save_WhenCancellationTokenIsCanceled_ThrowsOperationCancelledException(
            MemoryToggleCollectionCache cache,
            ToggleCollection toggleCollection,
            string etag
        )
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => cache.Save(toggleCollection, etag, cts.Token));
        }

        [Theory]
        [AutoMoqData]
        public async Task Load_WhenCancellationTokenIsCanceled_ThrowsOperationCancelledException(
            MemoryToggleCollectionCache cache
        )
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => cache.Load(cts.Token));
        }

        [Theory]
        [InlineAutoMoqData(false, false)]
        [InlineAutoMoqData(true,  false)]
        [InlineAutoMoqData(false, true)]
        public async Task Load_WhenTogglesOrEtagFileDoesNotExists_ReturnsEmptyResult(
            bool toggleCollectionExists,
            bool etagExists,
            [Frozen] MemoryToggleCollectionCacheSettings settings,
            [Frozen] MemoryCache memoryCache,
            MemoryToggleCollectionCache cache
        )
        {
            settings.EtagKeyName = "Etag";
            settings.ToggleCollectionKeyName = "Toggles";

            if (toggleCollectionExists)
            {
                var toggleCollection = new ToggleCollection();
                memoryCache.Set(settings.ToggleCollectionKeyName, toggleCollection);
            }

            if (toggleCollectionExists && etagExists)
            {
                var etag = Guid.NewGuid().ToString();
                memoryCache.Set(settings.EtagKeyName, etag);
            }

            var result = await cache.Load(CancellationToken.None);

            Assert.Equal(string.Empty, result.InitialETag);
            Assert.Null(result.InitialToggleCollection);
        }

        [Theory]
        [AutoMoqData]
        public async Task Load_WhenValidToggleAndEtagFilesExist_ReturnsExpectedResult(
            [Frozen] MemoryToggleCollectionCacheSettings settings,
            [Frozen] MemoryCache memoryCache,
            ToggleCollection toggleCollection,
            string etag
        )
        {
            settings.EtagKeyName = "Etag";
            settings.ToggleCollectionKeyName = "Toggles";

            memoryCache.Set(settings.ToggleCollectionKeyName, toggleCollection);
            memoryCache.Set(settings.EtagKeyName, etag);

            var cache = new MemoryToggleCollectionCache(settings, memoryCache);
            var result = await cache.Load(CancellationToken.None);

            Assert.Equal(etag, result.InitialETag);
            Assert.Same(toggleCollection, result.InitialToggleCollection);
        }
    }
}
