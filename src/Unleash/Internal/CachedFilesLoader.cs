using Unleash.Serialization;

namespace Unleash.Internal
{
    internal class CachedFilesLoader
    {
        private readonly IJsonSerializer jsonSerializer;
        private readonly IFileSystem fileSystem;
        private readonly IToggleBootstrapProvider toggleBootstrapProvider;
        private readonly string toggleFile;
        private readonly string etagFile;
        private readonly bool bootstrapOverride;

        public CachedFilesLoader(IJsonSerializer jsonSerializer, IFileSystem fileSystem, IToggleBootstrapProvider toggleBootstrapProvider, string toggleFile, string etagFile, bool bootstrapOverride = true)
        {
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleBootstrapProvider = toggleBootstrapProvider;
            this.toggleFile = toggleFile;
            this.etagFile = etagFile;
            this.bootstrapOverride = bootstrapOverride;
        }

        public CachedFilesResult EnsureExistsAndLoad()
        {
            var result = new CachedFilesResult();

            if (!fileSystem.FileExists(etagFile))
            {
                // Ensure files exists.
                fileSystem.WriteAllText(etagFile, string.Empty);
                result.InitialETag = string.Empty;
            }
            else
            {
                result.InitialETag = fileSystem.ReadAllText(etagFile);
            }

            // Toggles
            if (!fileSystem.FileExists(toggleFile))
            {
                fileSystem.WriteAllText(toggleFile, string.Empty);
                result.InitialToggleCollection = null;
            }
            else
            {
                using (var fileStream = fileSystem.FileOpenRead(toggleFile))
                {
                    result.InitialToggleCollection = jsonSerializer.Deserialize<ToggleCollection>(fileStream);
                }
            }

            if (result.InitialToggleCollection == null)
            {
                result.InitialETag = string.Empty;
            }

            if ((result.InitialToggleCollection == null || result.InitialToggleCollection.Features?.Count == 0 || bootstrapOverride) && toggleBootstrapProvider != null)
            {
                var bootstrapCollection = toggleBootstrapProvider.Read();
                if (bootstrapCollection != null && bootstrapCollection.Features?.Count > 0)
                    result.InitialToggleCollection = bootstrapCollection;
            }

            return result;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }
            public ToggleCollection InitialToggleCollection { get; set; }
        }
    }
}