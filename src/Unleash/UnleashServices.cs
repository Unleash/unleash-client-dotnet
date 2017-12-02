using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Metrics;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Strategies;

namespace Unleash
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

    internal class UnleashServices : IDisposable
    {
        internal CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        internal CancellationToken CancellationToken { get; }

        internal IUnleashApiClient ApiClient { get; set; }
        internal IUnleashScheduledTaskManager ScheduledTaskManager { get; set; }
        internal IUnleashContextProvider ContextProvider { get; set; }

        internal IFileSystem FileSystem { get; set; }
        internal MetricsBucket MetricsBucket { get; set; }
        internal ToggleCollectionSynchronization ToggleCollection {get;set;}

        internal string BackupFile { get; set; }
        internal string EtagBackupFile { get; set; }

        public bool IsMetricsDisabled { get; set; }

        public UnleashServices(UnleashSettings settings, Dictionary<string, IStrategy> strategyMap)
        {
            // Files
            var tempFolder = settings.LocalStorageFolder();
            BackupFile = Path.Combine(tempFolder, PrependFileName(settings, settings.FeatureToggleFilename));
            EtagBackupFile = Path.Combine(tempFolder, PrependFileName(settings, settings.EtagFilename));

            // Cancellation
            CancellationToken = CancellationTokenSource.Token;
            ContextProvider = settings.UnleashContextProvider;

            FileSystem = settings.FileSystem ?? new FileSystem(settings.Encoding);

            var loader = new CachedFilesLoader(settings.JsonSerializer, FileSystem, BackupFile, EtagBackupFile);
            var cachedFilesResult = loader.EnsureExistsAndLoad();

            ToggleCollection.Instance = cachedFilesResult.InitialToggleCollection ?? new ToggleCollection();

            MetricsBucket = new MetricsBucket();

            if (settings.UnleashApiClient == null)
            {
                var httpClient = settings.HttpClientFactory.Create(settings.UnleashApi);
                ApiClient = new UnleashApiClient(httpClient, settings.JsonSerializer, new UnleashApiClientRequestHeaders()
                {
                    AppName = settings.AppName,
                    InstanceId = settings.InstanceTag,
                    CustomHttpHeaders = settings.CustomHttpHeaders
                });
            }
            else
            {
                // Mocked backend: fill instance collection 
                ApiClient = settings.UnleashApiClient;
                var toggles = ApiClient.FetchToggles("", CancellationToken.None);
                ToggleCollection.Instance = toggles.Result.ToggleCollection;
            }

            ScheduledTaskManager = settings.ScheduledTaskManager;

            IsMetricsDisabled = settings.SendMetricsInterval == null;

            var fetchFeatureTogglesTask = new FetchFeatureTogglesTask(
                ApiClient, 
                ToggleCollection, 
                settings.JsonSerializer, 
                FileSystem, 
                BackupFile, 
                EtagBackupFile)
            {
                ExecuteDuringStartup = true,
                Interval = settings.FetchTogglesInterval,
                Etag = cachedFilesResult.InitialETag
            };


            var scheduledTasks = new List<IUnleashScheduledTask>(){
                fetchFeatureTogglesTask
            };

            if (settings.SendMetricsInterval != null)
            {
                var clientRegistrationBackgroundTask = new ClientRegistrationBackgroundTask(
                    ApiClient, 
                    settings, 
                    MetricsBucket, 
                    strategyMap.Select(pair => pair.Key).ToList())
                {
                    Interval = TimeSpan.Zero,
                    ExecuteDuringStartup = true
                };

                scheduledTasks.Add(clientRegistrationBackgroundTask);

                var clientMetricsBackgroundTask = new ClientMetricsBackgroundTask(
                    ApiClient, 
                    settings, 
                    MetricsBucket)
                {
                    ExecuteDuringStartup = false,
                    Interval = settings.SendMetricsInterval.Value
                };

                scheduledTasks.Add(clientMetricsBackgroundTask);
            }

            ScheduledTaskManager.Configure(scheduledTasks, CancellationToken);
        }

        private string PrependFileName(UnleashSettings settings, string filename)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            var extension = Path.GetExtension(filename);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            return new string($"{fileNameWithoutExtension}-{settings.AppName}-{settings.InstanceTag}-{settings.SdkVersion}{extension}"
                .Where(c => !invalidFileNameChars.Contains(c))
                .ToArray());
        }

        public void Dispose()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            ScheduledTaskManager?.Dispose();
            ToggleCollection?.Dispose();
        }
    }
}