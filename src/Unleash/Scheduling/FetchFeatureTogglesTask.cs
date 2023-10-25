using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;
using Unleash.Logging;
using Unleash.Events;
using System.Net.Http;

namespace Unleash.Scheduling
{
    internal class FetchFeatureTogglesTask : IUnleashScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));
        private readonly string toggleFile;
        private readonly string etagFile;

        private readonly IFileSystem fileSystem;
        private readonly EventCallbackConfig eventConfig;
        private readonly UnleashEngine engine;
        private readonly IUnleashApiClient apiClient;
        private readonly IJsonSerializer jsonSerializer;
        private readonly ThreadSafeToggleCollection toggleCollection;

        // In-memory reference of toggles/etags
        internal string Etag { get; set; }

        public FetchFeatureTogglesTask(
            IUnleashApiClient apiClient,
            ThreadSafeToggleCollection toggleCollection,
            IJsonSerializer jsonSerializer,
            IFileSystem fileSystem,
            EventCallbackConfig eventConfig,
            UnleashEngine engine,
            string toggleFile,
            string etagFile)
        {
            this.apiClient = apiClient;
            this.toggleCollection = toggleCollection;
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.eventConfig = eventConfig;
            this.engine = engine;
            this.toggleFile = toggleFile;
            this.etagFile = etagFile;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            FetchTogglesResult result;
            try
            {
                result = await apiClient.FetchToggles(Etag, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                Logger.ErrorException($"UNLEASH: Unhandled exception when fetching toggles.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = ex });
                throw new UnleashException("Exception while fetching from API", ex);
            }

            if (!result.HasChanged)
            {
                return;
            }

            if (string.IsNullOrEmpty(result.Etag))
                return;

            if (result.Etag == Etag)
                return;

            toggleCollection.Instance = result.ToggleCollection;

            // now that the toggle collection has been updated, raise the toggles updated event if configured
            eventConfig?.RaiseTogglesUpdated(new TogglesUpdatedEvent { UpdatedOn = DateTime.UtcNow });

            try
            {
                using (var fs = fileSystem.FileOpenCreate(toggleFile))
                {
                    jsonSerializer.Serialize(fs, result.ToggleCollection);
                }
            } 
            catch (IOException ex)
            {
                Logger.WarnException($"UNLEASH: Exception when writing to toggle file '{toggleFile}'.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesBackup, Error = ex });
            }

            Etag = result.Etag;

            try
            {
                fileSystem.WriteAllText(etagFile, Etag);
            }
            catch (IOException ex)
            {
                Logger.WarnException($"UNLEASH: Exception when writing to ETag file '{etagFile}'.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesBackup, Error = ex });
            }
        }

        public string Name => "fetch-feature-toggles-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}