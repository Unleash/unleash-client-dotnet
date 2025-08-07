using System;
using System.IO;
using Unleash.Events;
using Unleash.Logging;
using Unleash.Scheduling;
using Unleash.Serialization;

namespace Unleash.Internal
{
    internal class CachedFilesLoader
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));
        private readonly IJsonSerializer jsonSerializer;
        private readonly IFileSystem fileSystem;
        private readonly IToggleBootstrapProvider toggleBootstrapProvider;
        private readonly EventCallbackConfig eventConfig;
        private readonly IUnleashSettings settings;
        private readonly bool bootstrapOverride;

        public CachedFilesLoader(
            IJsonSerializer jsonSerializer,
            IFileSystem fileSystem,
            IToggleBootstrapProvider toggleBootstrapProvider,
            EventCallbackConfig eventConfig,
            IUnleashSettings settings,
            bool bootstrapOverride = true)
        {
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleBootstrapProvider = toggleBootstrapProvider;
            this.eventConfig = eventConfig;
            this.settings = settings;
            this.bootstrapOverride = bootstrapOverride;
        }

        public CachedFilesResult EnsureExistsAndLoad()
        {
            string toggleFilePath = settings.GetFeatureToggleFilePath();
            string etagFilePath = settings.GetFeatureToggleETagFilePath();

            string legacyToggleFilePath = settings.GetLegacyFeatureToggleFilePath();
            string legacyEtagFilePath = settings.GetLegacyFeatureToggleETagFilePath();

            var result = new CachedFilesResult
            {
                InitialETag = EnsureFileAndReadWithLegacy(etagFilePath, legacyEtagFilePath, string.Empty),
                InitialToggleCollection = EnsureFileAndReadJsonWithLegacy<ToggleCollection>(toggleFilePath, legacyToggleFilePath)
            };

            if (result.InitialToggleCollection == null)
            {
                result.InitialETag = string.Empty;
            }

            if (NeedsBootstrap(result.InitialToggleCollection) && toggleBootstrapProvider != null)
            {
                var bootstrapCollection = toggleBootstrapProvider.Read();
                if (bootstrapCollection?.Features?.Count > 0)
                {
                    result.InitialToggleCollection = bootstrapCollection;
                }
            }

            return result;
        }

        private string EnsureFileAndReadWithLegacy(string primaryPath, string legacyPath, string defaultContent)
        {
            if (fileSystem.FileExists(primaryPath))
            {
                return SafeRead(primaryPath, defaultContent);
            }

            if (fileSystem.FileExists(legacyPath))
            {
                return SafeRead(legacyPath, defaultContent);
            }

            return SafeCreateAndReturn(primaryPath, defaultContent);
        }

        private T EnsureFileAndReadJsonWithLegacy<T>(string primaryPath, string legacyPath) where T : class
        {
            if (fileSystem.FileExists(primaryPath))
            {
                return SafeReadJson<T>(primaryPath);
            }

            if (fileSystem.FileExists(legacyPath))
            {
                return SafeReadJson<T>(legacyPath);
            }

            try
            {
                fileSystem.WriteAllText(primaryPath, string.Empty);
            }
            catch (IOException ex)
            {
                // Should get handled later when we try to write
                Logger.Debug(() => $"UNLEASH: Failed to create backup file: {primaryPath}", ex);
            }
            return null;
        }

        private string SafeRead(string path, string defaultContent)
        {
            try { return fileSystem.ReadAllText(path); }
            catch (IOException ex)
            {
                Logger.Warn(() => $"UNLEASH: Failed to read etag file '{path}'.", ex);
                return defaultContent;
            }
        }

        private string SafeCreateAndReturn(string path, string defaultContent)
        {
            try
            {
                fileSystem.WriteAllText(path, defaultContent);
            }
            catch (IOException ex)
            {
                Logger.Warn(() => $"UNLEASH: Failed to create backup file: {path}", ex);
            }
            return defaultContent;
        }

        private T SafeReadJson<T>(string path) where T : class
        {
            try
            {
                using (var stream = fileSystem.FileOpenRead(path))
                {
                    return jsonSerializer.Deserialize<T>(stream);
                }
            }
            catch (IOException ex)
            {
                Logger.Warn(() => $"UNLEASH: Failed to load backup file: {path}", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.FileCache, Error = ex });
                return null;
            }
        }

        private bool NeedsBootstrap(ToggleCollection toggleCollection)
        {
            return toggleCollection == null || toggleCollection.Features?.Count == 0 || bootstrapOverride;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }
            public ToggleCollection InitialToggleCollection { get; set; }
        }
    }
}
