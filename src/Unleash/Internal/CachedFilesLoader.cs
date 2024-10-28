using System.IO;
using Unleash.Events;
using Unleash.Logging;
using Unleash.Scheduling;

namespace Unleash.Internal
{
    internal class CachedFilesLoader
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));
        private readonly IFileSystem fileSystem;
        private readonly IToggleBootstrapProvider toggleBootstrapProvider;
        private readonly EventCallbackConfig eventConfig;
        private readonly string toggleFile;
        private readonly string etagFile;
        private readonly bool bootstrapOverride;

        public CachedFilesLoader(
            IFileSystem fileSystem,
            IToggleBootstrapProvider toggleBootstrapProvider,
            EventCallbackConfig eventConfig,
            string toggleFile,
            string etagFile,
            bool bootstrapOverride = true)
        {
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
                    Logger.Error(() => $"UNLEASH: Unhandled exception when writing to ETag file '{etagFile}'.", ex);
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
                    Logger.Error(() => $"UNLEASH: Unhandled exception when reading from ETag file '{etagFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            // Toggles
            if (!fileSystem.FileExists(toggleFile))
            {
                try
                {
                    fileSystem.WriteAllText(toggleFile, string.Empty);
                    result.InitialState = string.Empty;
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"UNLEASH: Unhandled exception when writing to toggle file '{toggleFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }
            else
            {
                try
                {
                    result.InitialState = fileSystem.ReadAllText(toggleFile);
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"UNLEASH: Unhandled exception when reading from toggle file '{toggleFile}'.", ex);
                    eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            if (string.IsNullOrEmpty(result.InitialState))
            {
                result.InitialETag = string.Empty;
            }

            if ((string.IsNullOrEmpty(result.InitialState) || bootstrapOverride) && toggleBootstrapProvider != null)
            {
                var bootstrapState = toggleBootstrapProvider.Read();
                if (!string.IsNullOrEmpty(bootstrapState))
                    result.InitialState = bootstrapState;
            }

            return result;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }
            public string InitialState { get; set; }
        }
    }
}