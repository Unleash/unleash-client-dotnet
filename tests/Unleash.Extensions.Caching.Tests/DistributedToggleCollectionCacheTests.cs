using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Distributed;
using Unleash.Caching;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Extensions.Caching.Tests
{
    public class DistributedToggleCollectionCacheTests
    {
        [Theory]
        [AutoMoqData]
        public async Task Save_WhenToggleCollectionIsNull_ThrowsArgumentNullException(
            DistributedToggleCollectionCache cache,
            string etag
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.Save(null, etag, CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Save_WhenEtagIsNull_ThrowsArgumentNullException(
            DistributedToggleCollectionCache cache,
            ToggleCollection toggleCollection
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.Save(toggleCollection, null, CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Save_WhenCancellationTokenIsCanceled_ThrowsOperationCancelledException(
            DistributedToggleCollectionCache cache,
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
            DistributedToggleCollectionCache cache
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
            [Frozen] DistributedToggleCollectionCacheSettings settings,
            [Frozen] MemoryDistributedCache distributedCache,
            DistributedToggleCollectionCache cache
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            settings.EtagKeyName = "Etag";
            settings.ToggleCollectionKeyName = "Toggles";

            if (toggleCollectionExists)
            {
                var toggleCollection = new ToggleCollection();

                using (var ms = new MemoryStream())
                {
                    jsonSerializer.Serialize(ms, toggleCollection);
                    ms.Seek(0, SeekOrigin.Begin);

                    await distributedCache.SetAsync(settings.ToggleCollectionKeyName, ms.ToArray(), settings.ToggleCollectionEntryOptions, CancellationToken.None).ConfigureAwait(false);
                }
            }

            if (toggleCollectionExists && etagExists)
            {
                var etag = Guid.NewGuid().ToString();
                distributedCache.SetString(settings.EtagKeyName, etag);
            }

            var result = await cache.Load(CancellationToken.None);

            Assert.Equal(string.Empty, result.InitialETag);
            Assert.Null(result.InitialToggleCollection);
        }

        [Theory]
        [AutoMoqData]
        public async Task Load_WhenValidToggleAndEtagFilesExist_ReturnsExpectedResult(
            [Frozen] MemoryDistributedCache distributedCache,
            ToggleCollection toggleCollection,
            string etag
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            var settings = new DistributedToggleCollectionCacheSettings
            {
                EtagEntryOptions =
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                },
                ToggleCollectionEntryOptions =
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                }
            };

            using (var ms = new MemoryStream())
            {
                jsonSerializer.Serialize(ms, toggleCollection);
                ms.Seek(0, SeekOrigin.Begin);

                await distributedCache.SetAsync(settings.ToggleCollectionKeyName, ms.ToArray(), settings.ToggleCollectionEntryOptions, CancellationToken.None).ConfigureAwait(false);
                await distributedCache.SetStringAsync(settings.EtagKeyName, etag, settings.EtagEntryOptions, CancellationToken.None).ConfigureAwait(false);
            }

            var cache = new DistributedToggleCollectionCache(settings, distributedCache, jsonSerializer);
            var result = await cache.Load(CancellationToken.None);

            Assert.Equal(etag, result.InitialETag);

            AssertionUtils.AssertToggleCollectionsEquivalent(toggleCollection, result.InitialToggleCollection);
        }
    }
}
