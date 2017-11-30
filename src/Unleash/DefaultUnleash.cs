using Unleash.Internal;

namespace Unleash
{
    using Logging;
    using Strategies;
    using System.Collections.Generic;

    /// <inheritdoc />
    public class DefaultUnleash : IUnleash
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultUnleash));

        private static readonly UnknownStrategy UnknownStrategy = new UnknownStrategy();

        private static readonly IStrategy[] DefaultStragegies = {
            new DefaultStrategy(),
            new UserWithIdStrategy(),
            new GradualRolloutUserIdStrategy(),
            new GradualRolloutRandomStrategy(),
            new ApplicationHostnameStrategy(),
            new GradualRolloutSessionIdStrategy(),
            new RemoteAddressStrategy(),
        };

        private readonly Dictionary<string, IStrategy> strategyMap;

        private readonly UnleashServices services;

        ///// <summary>
        ///// Initializes a new instance of Unleash client. 
        ///// </summary>
        ///// <param name="config">Unleash settings</param>
        ///// <param name="strategies">Available strategies. When none defined, all default strategies will be added.</param>
        public DefaultUnleash(UnleashSettings settings, params IStrategy[] strategies)
        {
            var settingsValidator = new UnleashSettingsValidator();
            settingsValidator.Validate(settings);
            
            strategyMap = BuildStrategyMap(DefaultStragegies, strategies);

            services = new UnleashServices(settings, strategyMap);

            Logger.Info($"UNLEASH: Unleash is initialized and configured with: {settings}");
        }

        /// <inheritdoc />
        public bool IsEnabled(string toggleName)
        {
            return IsEnabled(toggleName, false);
        }

        /// <inheritdoc />
        public bool IsEnabled(string toggleName, bool defaultSetting)
        {
            return CheckIsEnabled(toggleName, services.ContextProvider.Context, defaultSetting);
        }

        private bool CheckIsEnabled(string toggleName, UnleashContext context, bool defaultSetting)
        {
            var featureToggle = services.ToggleCollectionInstance.ToggleCollection.GetToggleByName(toggleName);

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
            if (services.IsMetricsDisabled)
                return;

            services.MetricsBucket.RegisterCount(toggleName, enabled);
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
            services?.Dispose(); 
        }
    }
}