using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Unleash.Caching;
using Unleash.Communication;
using Unleash.Metrics;
using Unleash.Scheduling;
using Unleash.Strategies;

namespace Unleash.Internal
{
    internal class UnleashServices : IUnleashServices
    {
        public IReadOnlyDictionary<string, IStrategy> StrategyMap { get; }

        internal CancellationToken CancellationToken { get; }
        internal ThreadSafeToggleCollection ToggleCollection { get; }
        internal ThreadSafeMetricsBucket MetricsBucket { get; }

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly IUnleashScheduledTaskManager scheduledTaskManager;

        public UnleashServices(UnleashSettings settings, IUnleashApiClientFactory unleashApiClientFactory,
            IUnleashScheduledTaskManager scheduledTaskManager, IToggleCollectionCache toggleCollectionCache,
            IEnumerable<IStrategy> strategies)
        {
            this.scheduledTaskManager = scheduledTaskManager ?? throw new ArgumentNullException(nameof(scheduledTaskManager));

            var settingsValidator = new UnleashSettingsValidator();
            settingsValidator.Validate(settings);

            StrategyMap = BuildStrategyMap(strategies?.ToArray() ?? new IStrategy[0]);

            CancellationToken = cancellationTokenSource.Token;

            var cachedFilesResult = toggleCollectionCache.Load(cancellationTokenSource.Token).GetAwaiter().GetResult();

            ToggleCollection = new ThreadSafeToggleCollection
            {
                Instance = cachedFilesResult.InitialToggleCollection ?? new ToggleCollection()
            };

            MetricsBucket = new ThreadSafeMetricsBucket();

            var scheduledTasks = CreateScheduledTasks(settings, unleashApiClientFactory, toggleCollectionCache, cachedFilesResult);

            scheduledTaskManager.Configure(scheduledTasks, CancellationToken);
        }

        private IEnumerable<IUnleashScheduledTask> CreateScheduledTasks(UnleashSettings settings,
            IUnleashApiClientFactory unleashApiClientFactory, IToggleCollectionCache toggleCollectionCache,
            ToggleCollectionCacheResult toggleCollectionCacheResult)
        {
            yield return new FetchFeatureTogglesTask(
                unleashApiClientFactory,
                ToggleCollection,
                toggleCollectionCache)
            {
                ExecuteDuringStartup = true,
                Interval = settings.FetchTogglesInterval,
                Etag = toggleCollectionCacheResult.InitialETag
            };

            if (settings.SendMetricsInterval == null) yield break;

            yield return new ClientRegistrationBackgroundTask(
                unleashApiClientFactory,
                settings,
                StrategyMap.Keys.ToList())
            {
                Interval = TimeSpan.Zero,
                ExecuteDuringStartup = true
            };

            yield return new ClientMetricsBackgroundTask(
                unleashApiClientFactory,
                settings,
                MetricsBucket)
            {
                ExecuteDuringStartup = false,
                Interval = settings.SendMetricsInterval.Value
            };
        }

        private static IReadOnlyDictionary<string, IStrategy> BuildStrategyMap(IStrategy[] strategies)
        {
            var map = new Dictionary<string, IStrategy>(strategies.Length);

            foreach (var strategy in strategies)
                map.Add(strategy.Name, strategy);

            return new ReadOnlyDictionary<string, IStrategy>(map);
        }

        /// <inheritdoc />
        public ToggleCollection GetToggleCollection()
        {
            return ToggleCollection.Instance;
        }

        /// <inheritdoc />
        public void RegisterCount(string toggleName, bool enabled)
        {
            MetricsBucket.RegisterCount(toggleName, enabled);
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
    }
}
