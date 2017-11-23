using System;
using System.Linq;
using System.Threading;
using Unleash.Logging;
using Unleash.Metrics;

namespace Unleash
{
    using Strategies;
    using Util;
    using Repository;
    using System.Collections.Generic;

    public class DefaultUnleash : IUnleash
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultUnleash));

        private static readonly UnknownStrategy UnknownStrategy = new UnknownStrategy();

        private static readonly IStrategy[] DefaultStragegies = {
            new DefaultStrategy(),
            new ApplicationHostnameStrategy(),
            new GradualRolloutRandomStrategy(),
            new GradualRolloutSessionIdStrategy(),
            new GradualRolloutUserIdStrategy(),
            new RemoteAddressStrategy(),
            new UserWithIdStrategy(),
        };

        private readonly List<TimerTaskRunner> tasksRunners = new List<TimerTaskRunner>();
        private readonly ToggleCollectionInstance toggleRepository;
        private readonly Dictionary<string, IStrategy> strategyMap;
        private readonly UnleashConfig unleashConfig;
        
        public DefaultUnleash(UnleashConfig unleashConfig, params IStrategy[] strategies)
        {
            this.unleashConfig = unleashConfig;
            this.unleashConfig.ValidateUserInputAndSetDefaults();

            Logger.Info($"Unleash is configured with: {unleashConfig}");

            this.strategyMap = BuildStrategyMap(DefaultStragegies, strategies);
            toggleRepository = new ToggleCollectionInstance(unleashConfig);

            RegisterBackgroundTask(new ClientRegistrationBackgroundTask(unleashConfig, strategyMap.Select(pair => pair.Key).ToList()), TimeSpan.Zero, true, unleashConfig.Services.CancellationToken );
            RegisterBackgroundTask(new ClientMetricsBackgroundTask(unleashConfig), unleashConfig.SendMetricsInterval, false, unleashConfig.Services.CancellationToken );
            RegisterBackgroundTask(new FetchFeatureTogglesTask(unleashConfig, toggleRepository), TimeSpan.FromSeconds(20), true, unleashConfig.Services.CancellationToken);
        }

        internal DefaultUnleash(UnleashConfig unleashConfig, ToggleCollectionInstance toggleRepository, params IStrategy[] strategies)
        {
            this.unleashConfig = unleashConfig;
            this.toggleRepository = toggleRepository;
            this.strategyMap = BuildStrategyMap(DefaultStragegies, strategies);
            toggleRepository = new ToggleCollectionInstance(unleashConfig);

            RegisterBackgroundTask(new ClientRegistrationBackgroundTask(unleashConfig, strategyMap.Select(pair => pair.Key).ToList()), TimeSpan.Zero, true, unleashConfig.Services.CancellationToken);
            RegisterBackgroundTask(new ClientMetricsBackgroundTask(unleashConfig), unleashConfig.SendMetricsInterval, false, unleashConfig.Services.CancellationToken);
            RegisterBackgroundTask(new FetchFeatureTogglesTask(unleashConfig, toggleRepository), TimeSpan.FromSeconds(20), true, unleashConfig.Services.CancellationToken);
        }

        private void RegisterBackgroundTask(IBackgroundTask task, TimeSpan interval, bool executeImmediatly, CancellationToken cancellationToken)
        {
            var taskRunner = new TimerTaskRunner(task, interval, executeImmediatly, cancellationToken);
            tasksRunners.Add(taskRunner);
        }

        public bool IsEnabled(string toggleName)
        {
            return IsEnabled(toggleName, false);
        }

        public bool IsEnabled(string toggleName, bool defaultSetting)
        {
            return isEnabled(toggleName, unleashConfig.ContextProvider.Context, defaultSetting);
        }

        private bool isEnabled(string toggleName, UnleashContext context, bool defaultSetting)
        {
            var featureToggle = toggleRepository.ToggleCollection.GetToggleByName(toggleName);

            bool enabled = false;
            if (featureToggle == null)
            {
                enabled = defaultSetting;
            }
            else if(!featureToggle.Enabled)
            {
                // Overall false
                enabled = false;
            }
            else
            {
                for (var i = 0; i < featureToggle.Strategies.Count; i++)
                {
                    var toggleStrategy = featureToggle.Strategies[i];
                    var strategy = GetStrategyOrUnknown(toggleStrategy.Name);

                    if (!strategy.IsEnabled(toggleStrategy.Parameters, context))
                        continue;

                    enabled = true;
                    break;
                }
            }

            RegisterCount(toggleName, enabled);
            return enabled;
        }

        private void RegisterCount(string toggleName, bool enabled)
        {
            if (unleashConfig.IsMetricsDisabled)
                return;

            unleashConfig.Services.MetricsBucket.RegisterCount(toggleName, enabled);
        }

        private static Dictionary<string, IStrategy> BuildStrategyMap(IStrategy[] defaultStragegies, IStrategy[] strategies)
        {
            if (strategies != null && strategies.Length > 0)
            {
                var map = new Dictionary<string, IStrategy>(strategies.Length);

                foreach (var strategy in strategies)
                    map.Add(strategy.Name, strategy);

                return map;
            }
            else
            {
                var map = new Dictionary<string, IStrategy>(defaultStragegies.Length);

                foreach (var strategy in defaultStragegies)
                    map.Add(strategy.Name, strategy);

                return map;
            }
        }

        private IStrategy GetStrategyOrUnknown(string strategy)
        {
            return strategyMap.ContainsKey(strategy) 
                ? strategyMap[strategy] 
                : UnknownStrategy;
        }

        public void Dispose()
        {
            if (!unleashConfig.Services.CancellationTokenSource.IsCancellationRequested)
            {
                unleashConfig.Services.CancellationTokenSource.Cancel();
            }

            foreach (var taskRunner in tasksRunners)
                taskRunner.Dispose();

            // Avoid disposing timers twice.
            tasksRunners.Clear();
        }
    }
}