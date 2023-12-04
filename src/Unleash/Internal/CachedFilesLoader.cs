using System.IO;
using System.Text;
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
        private readonly string toggleFile;
        private readonly string etagFile;
        private readonly bool bootstrapOverride;

        public CachedFilesLoader(
            IJsonSerializer jsonSerializer,
            IFileSystem fileSystem,
            IToggleBootstrapProvider toggleBootstrapProvider,
            EventCallbackConfig eventConfig,
            string toggleFile,
            string etagFile,
            bool bootstrapOverride = true)
        {
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleBootstrapProvider = toggleBootstrapProvider;
            this.eventConfig = eventConfig;
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
                try
                {
                    fileSystem.WriteAllText(etagFile, string.Empty);
                    result.InitialETag = string.Empty;
                }
                catch (IOException ex)
                {
                    Logger.ErrorException($"UNLEASH: Unhandled exception when writing to ETag file '{etagFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }
            else
            {
                try
                {
                    result.InitialETag = fileSystem.ReadAllText(etagFile);
                }
                catch (IOException ex)
                {
                    Logger.ErrorException($"UNLEASH: Unhandled exception when reading from ETag file '{etagFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            // Toggles
            if (!fileSystem.FileExists(toggleFile))
            {
                try
                {
                    fileSystem.WriteAllText(toggleFile, string.Empty);
                    result.InitialToggleCollection = null;
                }
                catch (IOException ex)
                {
                    Logger.ErrorException($"UNLEASH: Unhandled exception when writing to toggle file '{toggleFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }
            else
            {
                try
                {
                    var fileContent = fileSystem.ReadAllText(toggleFile);
                    result.ToggleContent = fileContent;
                    using (var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
                    {
                        result.InitialToggleCollection = jsonSerializer.Deserialize<ToggleCollection>(fileStream);
                    }
                }
                catch (IOException ex)
                {
                    Logger.ErrorException($"UNLEASH: Unhandled exception when reading from toggle file '{toggleFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            if (result.InitialToggleCollection == null)
            {
                result.InitialETag = string.Empty;
            }

            if ((result.InitialToggleCollection == null ||
                    result.InitialToggleCollection.Features?.Count == 0 ||
                    bootstrapOverride
                ) &&
                toggleBootstrapProvider != null)
            {
                var bootstrapResult = toggleBootstrapProvider.Read();
                if (bootstrapResult != null &&
                    bootstrapResult.ToggleCollection != null &&
                    bootstrapResult.ToggleCollection.Features?.Count > 0)
                {
                    result.InitialToggleCollection = bootstrapResult.ToggleCollection;
                    result.ToggleContent = bootstrapResult.ToggleContent;
                }
            }

            return result;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }
            public ToggleCollection InitialToggleCollection { get; set; }
            public string ToggleContent { get; set; }
        }
    }
}