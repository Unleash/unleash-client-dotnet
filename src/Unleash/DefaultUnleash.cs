using System;
using System.Linq;
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

            Logger.Info($"UNLEASH: Unleash is initialized and configured with: {unleashConfig}");

            strategyMap = BuildStrategyMap(DefaultStragegies, strategies);
            toggleRepository = new ToggleCollectionInstance(unleashConfig);

            if (!unleashConfig.DisableBackgroundTasks)
            {
                RegisterBackgroundTask(new FetchFeatureTogglesTask(unleashConfig, toggleRepository), unleashConfig.FetchTogglesInterval, executeImmediatly: true);
                RegisterBackgroundTask(new ClientRegistrationBackgroundTask(unleashConfig, strategyMap.Select(pair => pair.Key).ToList()), TimeSpan.Zero, executeImmediatly: true);
                RegisterBackgroundTask(new ClientMetricsBackgroundTask(unleashConfig), unleashConfig.SendMetricsInterval, executeImmediatly: false);
            }
        }

        private void RegisterBackgroundTask(IBackgroundTask backgroundTask, TimeSpan interval, bool executeImmediatly)
        {
            var taskRunner = new TimerTaskRunner(
                backgroundTask, 
                interval, 
                executeImmediatly, 
                unleashConfig.Services.CancellationToken);

            tasksRunners.Add(taskRunner);
        }

        public bool IsEnabled(string toggleName)
        {
            return IsEnabled(toggleName, false);
        }

        public bool IsEnabled(string toggleName, bool defaultSetting)
        {
            return CheckIsEnabled(toggleName, unleashConfig.ContextProvider.Context, defaultSetting);
        }

        private bool CheckIsEnabled(string toggleName, UnleashContext context, bool defaultSetting)
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