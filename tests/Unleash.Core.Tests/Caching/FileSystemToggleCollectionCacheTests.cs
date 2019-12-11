using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using Unleash.Caching;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Core.Tests.Caching
{
    public class FileSystemToggleCollectionCacheTests
    {
        [Theory]
        [AutoMoqData]
        public async Task Save_WhenToggleCollectionIsNull_ThrowsArgumentNullException(
            FileSystemToggleCollectionCache cache,
            string etag
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.Save(null, etag, CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Save_WhenEtagIsNull_ThrowsArgumentNullException(
            FileSystemToggleCollectionCache cache,
            ToggleCollection toggleCollection
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.Save(toggleCollection, null, CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Save_WhenCancellationTokenIsCanceled_ThrowsOperationCancelledException(
            FileSystemToggleCollectionCache cache,
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
            FileSystemToggleCollectionCache cache
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
            bool toggleFileExists,
            bool etagFileExists,
            [Frozen] UnleashSettings settings,
            [Frozen] Mock<IFileSystem> fileSystem,
            FileSystemToggleCollectionCache cache
        )
        {
            var etagFilePath = settings.GetFeatureToggleETagFilePath();
            var toggleFilePath = settings.GetFeatureToggleFilePath();

            fileSystem.Setup(fs => fs.FileExists(etagFilePath)).Returns(etagFileExists);

            if (etagFileExists)
            {
                fileSystem.Setup(fs => fs.FileExists(toggleFilePath)).Returns(toggleFileExists);
            }

            var result = await cache.Load(CancellationToken.None);

            Assert.Equal(string.Empty, result.InitialETag);
            Assert.Null(result.InitialToggleCollection);

            fileSystem.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Load_WhenValidToggleAndEtagFilesExist_ReturnsExpectedResult(
            [Frozen] UnleashSettings settings,
            [Frozen] Mock<IFileSystem> fileSystem,
            [Frozen] Mock<IJsonSerializer> jsonSerializer,
            FileSystemToggleCollectionCache cache,
            ToggleCollection toggleCollection,
            string etag
        )
        {
            var toggleFilePath = settings.GetFeatureToggleFilePath();
            var etagFilePath = settings.GetFeatureToggleETagFilePath();

            fileSystem.Setup(fs => fs.FileExists(toggleFilePath)).Returns(true);
            fileSystem.Setup(fs => fs.FileExists(etagFilePath)).Returns(true);

            var ms = new MemoryStream();

            fileSystem.Setup(fs => fs.FileOpenRead(toggleFilePath)).Returns(ms);
            fileSystem.Setup(fs => fs.ReadAllText(etagFilePath)).Returns(etag);

            jsonSerializer.Setup(js => js.Deserialize<ToggleCollection>(ms)).Returns(toggleCollection);

            var result = await cache.Load(CancellationToken.None);

            Assert.Equal(etag, result.InitialETag);
            Assert.Same(toggleCollection, result.InitialToggleCollection);

            fileSystem.VerifyAll();
            jsonSerializer.VerifyAll();
        }
    }
}
