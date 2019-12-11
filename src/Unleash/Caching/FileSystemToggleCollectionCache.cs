using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Caching
{
    public class FileSystemToggleCollectionCache : IToggleCollectionCache
    {
        private readonly IJsonSerializer jsonSerializer;
        private readonly IFileSystem fileSystem;
        private readonly string toggleFile;
        private readonly string etagFile;

        public FileSystemToggleCollectionCache(UnleashSettings settings, IJsonSerializer jsonSerializer, IFileSystem fileSystem)
        {
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleFile = settings.GetFeatureToggleFilePath();
            this.etagFile = settings.GetFeatureToggleETagFilePath();
        }

        /// <inheritdoc />
        public Task Save(ToggleCollection toggleCollection, string etag, CancellationToken cancellationToken)
        {
            if (toggleCollection == null) throw new ArgumentNullException(nameof(toggleCollection));
            if (etag == null) throw new ArgumentNullException(nameof(etag));

            cancellationToken.ThrowIfCancellationRequested();

            using (var fs = fileSystem.FileOpenCreate(toggleFile))
            {
                jsonSerializer.Serialize(fs, toggleCollection);
            }

            fileSystem.WriteAllText(etagFile, etag);
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task<ToggleCollectionCacheResult> Load(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!fileSystem.FileExists(etagFile) || !fileSystem.FileExists(toggleFile))
            {
                return Task.FromResult(ToggleCollectionCacheResult.CacheMiss);
            }

            using (var fileStream = fileSystem.FileOpenRead(toggleFile))
            {
                try
                {
                    var initialToggleCollection = jsonSerializer.Deserialize<ToggleCollection>(fileStream);
                    var initialETag = fileSystem.ReadAllText(etagFile);
                    return Task.FromResult(ToggleCollectionCacheResult.FromResult(initialToggleCollection, initialETag));
                }
                catch
                {
                    return Task.FromResult(ToggleCollectionCacheResult.CacheMiss);
                }
            }
        }
    }
}
