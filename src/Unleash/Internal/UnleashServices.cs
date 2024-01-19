using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Metrics;
using Unleash.Scheduling;
using Unleash.Strategies;
using Unleash.Events;
using System.Threading.Tasks;

namespace Unleash
{
    internal class UnleashServices : IDisposable
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly IUnleashScheduledTaskManager scheduledTaskManager;

        private DateTime? LastUpdated;
        private string backupFile;
        const string supportedSpecVersion = "4.5.1";

        internal CancellationToken CancellationToken { get; }
        internal IUnleashContextProvider ContextProvider { get; }
        internal ThreadSafeToggleCollection ToggleCollection { get; }
        internal bool IsMetricsDisabled { get; }

        private readonly FetchFeatureToggles fetchFeatureToggles;
        private readonly FetchFeatureTogglesTask fetchFeatureTogglesTask;
        private readonly UnleashSettings settings;
        private readonly EventCallbackConfig eventConfig;

        internal ThreadSafeMetricsBucket MetricsBucket { get; }
        internal FetchFeatureTogglesTask FetchFeatureTogglesTask { get; }

        public UnleashServices(UnleashSettings settings, EventCallbackConfig eventConfig, Dictionary<string, IStrategy> strategyMap)
        {
            if (settings.FileSystem == null)
            {
                settings.FileSystem = new FileSystem(settings.Encoding);
            }

            backupFile = settings.GetFeatureToggleFilePath();
            var etagBackupFile = settings.GetFeatureToggleETagFilePath();

            // Cancellation
            CancellationToken = cancellationTokenSource.Token;
            ContextProvider = settings.UnleashContextProvider;

            var loader = new CachedFilesLoader(settings.JsonSerializer, settings.FileSystem, settings.ToggleBootstrapProvider, eventConfig, backupFile, etagBackupFile, settings.BootstrapOverride);
            var cachedFilesResult = loader.EnsureExistsAndLoad();

            ToggleCollection = new ThreadSafeToggleCollection
            {
                Instance = cachedFilesResult.InitialToggleCollection ?? new ToggleCollection()
            };

            MetricsBucket = new ThreadSafeMetricsBucket();

            IUnleashApiClient apiClient;
            if (settings.UnleashApiClient == null)
            {
                var uri = settings.UnleashApi;
                if (!uri.AbsolutePath.EndsWith("/"))
                {
                    uri = new Uri($"{uri.AbsoluteUri}/");
                }

                var httpClient = settings.HttpClientFactory.Create(uri);
                apiClient = new UnleashApiClient(httpClient, settings.JsonSerializer, new UnleashApiClientRequestHeaders()
                {
                    AppName = settings.AppName,
                    InstanceTag = settings.InstanceTag,
                    CustomHttpHeaders = settings.CustomHttpHeaders,
                    CustomHttpHeaderProvider = settings.UnleashCustomHttpHeaderProvider,
                    SupportedSpecVersion = supportedSpecVersion
                }, eventConfig, settings.ProjectId);
            }
            else
            {
                // Mocked backend: fill instance collection 
                apiClient = settings.UnleashApiClient;
            }

            scheduledTaskManager = settings.ScheduledTaskManager;

            IsMetricsDisabled = settings.SendMetricsInterval == null;

            fetchFeatureToggles = new FetchFeatureToggles(apiClient, eventConfig);

            fetchFeatureTogglesTask = new FetchFeatureTogglesTask(
                fetchFeatureToggles,
                OnFeatureFlagsUpdatedChron
                )
            {
                ExecuteDuringStartup = settings.ScheduleFeatureToggleFetchImmediatly,
                Interval = settings.FetchTogglesInterval,
                Etag = cachedFilesResult.InitialETag,
                Enabled = false
            };
            FetchFeatureTogglesTask = fetchFeatureTogglesTask;

            var scheduledTasks = new List<IUnleashScheduledTask>(){
                fetchFeatureTogglesTask
            };

            if (settings.SendMetricsInterval != null)
            {
                var clientRegistrationBackgroundTask = new ClientRegistrationBackgroundTask(
                    apiClient, 
                    settings,
                    strategyMap.Select(pair => pair.Key).ToList())
                {
                    Interval = TimeSpan.Zero,
                    ExecuteDuringStartup = true
                };

                scheduledTasks.Add(clientRegistrationBackgroundTask);

                var clientMetricsBackgroundTask = new ClientMetricsBackgroundTask(
                    apiClient, 
                    settings, 
                    MetricsBucket)
                {
                    Interval = settings.SendMetricsInterval.Value
                };

                scheduledTasks.Add(clientMetricsBackgroundTask);
            }

            scheduledTaskManager.Configure(scheduledTasks, CancellationToken);
            this.settings = settings;
            this.eventConfig = eventConfig;
        }

        private void OnFeatureFlagsUpdatedChron(ToggleCollection toggleCollection, string etag)
        {
            ToggleCollection.Instance = toggleCollection;

             // now that the toggle collection has been updated, raise the toggles updated event if configured
            eventConfig?.RaiseTogglesUpdated(new TogglesUpdatedEvent { UpdatedOn = DateTime.UtcNow });

            StoreState(toggleCollection, etag);
        }

        private void StoreState(ToggleCollection toggleCollection, string etag)
        {
            LastUpdated = DateTime.UtcNow;

            try
            {
                using (var fs = settings.FileSystem.FileOpenCreate(backupFile))
                {
                    settings.JsonSerializer.Serialize(fs, toggleCollection);
                }
            }
            catch (IOException ex)
            {
                Logger.Warn(() => $"UNLEASH: Exception when writing to toggle file '{backupFile}'.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesBackup, Error = ex });
            }

            try
            {
                settings.FileSystem.WriteAllText(settings.EtagFilename, etag);
            }
            catch (IOException ex)
            {
                Logger.Warn(() => $"UNLEASH: Exception when writing to ETag file '{settings.EtagFilename}'.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesBackup, Error = ex });
            }
        }

        public void Dispose()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }

            scheduledTaskManager?.Dispose();
            ToggleCollection?.Dispose();
        }

        public async Task<ThreadSafeToggleCollection> GetFeatureFlags()
        {
            if (LastUpdated == null)
            {
                (ToggleCollection collection, string etag, bool hasChanged) = await fetchFeatureToggles.FetchToggles(new CancellationToken());
                ToggleCollection.Instance = collection;
                StoreState(collection, etag);
                fetchFeatureTogglesTask.Enabled = true;
            }

            return ToggleCollection;
        }
    }
}