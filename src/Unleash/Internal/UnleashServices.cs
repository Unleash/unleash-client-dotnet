using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unleash.Communication;
using Unleash.Events;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Scheduling;
using Unleash.Strategies;
using Yggdrasil;

namespace Unleash
{
    internal class UnleashServices : IDisposable
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(UnleashServices));
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly IUnleashScheduledTaskManager scheduledTaskManager;
        private readonly string connectionId = Guid.NewGuid().ToString();

        public const string supportedSpecVersion = "5.1.9";

        internal CancellationToken CancellationToken { get; }
        internal IUnleashContextProvider ContextProvider { get; }
        internal bool IsMetricsDisabled { get; }
        internal FetchFeatureTogglesTask FetchFeatureTogglesTask { get; }
        internal YggdrasilEngine engine { get; }
        private static readonly IList<string> DefaultStrategyNames = new List<string> {
            "applicationHostname",
            "default",
            "flexibleRollout",
            "gradualRolloutRandom",
            "gradualRolloutSessionId",
            "gradualRolloutUserId",
            "remoteAddress",
            "userWithId"
        };

        public UnleashServices(UnleashSettings settings, EventCallbackConfig eventConfig, List<Strategies.IStrategy> strategies = null)
        {
            if (settings.FileSystem == null)
            {
                settings.FileSystem = new FileSystem(settings.Encoding);
            }

            List<Yggdrasil.IStrategy> yggdrasilStrategies = strategies?.Select(s => new CustomStrategyAdapter(s)).Cast<Yggdrasil.IStrategy>().ToList();

            engine = new YggdrasilEngine(yggdrasilStrategies);

            var backupFile = settings.GetFeatureToggleFilePath();
            var etagBackupFile = settings.GetFeatureToggleETagFilePath();

            // Cancellation
            CancellationToken = cancellationTokenSource.Token;
            ContextProvider = settings.UnleashContextProvider;

            var loader = new CachedFilesLoader(settings.FileSystem, settings.ToggleBootstrapProvider, eventConfig, backupFile, etagBackupFile, settings.BootstrapOverride);
            var cachedFilesResult = loader.EnsureExistsAndLoad();

            if (!string.IsNullOrEmpty(cachedFilesResult.InitialState))
            {
                try
                {
                    engine.TakeState(cachedFilesResult.InitialState);
                }
                catch (Exception ex)
                {
                    Logger.Error(() => $"UNLEASH: Failed to load initial state from file: {ex.Message}");
                    eventConfig.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            IUnleashApiClient apiClient;
            if (settings.UnleashApiClient == null)
            {
                var uri = settings.UnleashApi;
                if (!uri.AbsolutePath.EndsWith("/"))
                {
                    uri = new Uri($"{uri.AbsoluteUri}/");
                }

                var httpClient = settings.HttpClientFactory.Create(uri);
                apiClient = new UnleashApiClient(httpClient, new UnleashApiClientRequestHeaders()
                {
                    AppName = settings.AppName,
                    InstanceTag = settings.InstanceTag,
                    ConnectionId = connectionId,
                    SdkVersion = settings.SdkVersion,
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

            var fetchFeatureTogglesTask = new FetchFeatureTogglesTask(
                engine,
                apiClient,
                settings.FileSystem,
                eventConfig,
                backupFile,
                etagBackupFile,
                settings.ThrowOnInitialFetchFail)
            {
                ExecuteDuringStartup = settings.ScheduleFeatureToggleFetchImmediatly,
                Interval = settings.FetchTogglesInterval,
                Etag = cachedFilesResult.InitialETag
            };
            FetchFeatureTogglesTask = fetchFeatureTogglesTask;

            var scheduledTasks = new List<IUnleashScheduledTask>(){
                fetchFeatureTogglesTask
            };

            if (settings.SendMetricsInterval != null)
            {
                var strategyNames = (strategies == null ? DefaultStrategyNames : DefaultStrategyNames.Concat(strategies.Select(s => s.Name))).ToList();

                var clientRegistrationBackgroundTask = new ClientRegistrationBackgroundTask(
                    apiClient,
                    settings,
                    strategyNames)
                {
                    Interval = TimeSpan.Zero,
                    ExecuteDuringStartup = true
                };

                scheduledTasks.Add(clientRegistrationBackgroundTask);

                var clientMetricsBackgroundTask = new ClientMetricsBackgroundTask(
                    engine,
                    apiClient,
                    settings
                    )
                {
                    Interval = settings.SendMetricsInterval.Value
                };

                scheduledTasks.Add(clientMetricsBackgroundTask);
            }

            scheduledTaskManager.Configure(scheduledTasks, CancellationToken);
        }

        public void Dispose()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }

            engine?.Dispose();
            scheduledTaskManager?.Dispose();
        }
    }
}