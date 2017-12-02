using Unleash.Serialization;

namespace Unleash.Internal
{
    internal class CachedFilesLoader
    {
        private readonly IJsonSerializer jsonSerializer;
        private readonly IFileSystem fileSystem;
        private readonly string toggleFile;
        private readonly string etagFile;

        public CachedFilesLoader(IJsonSerializer jsonSerializer, IFileSystem fileSystem, string toggleFile, string etagFile)
        {
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleFile = toggleFile;
            this.etagFile = etagFile;
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

            return result;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }
            public ToggleCollection InitialToggleCollection { get; set; }
        }
    }
}