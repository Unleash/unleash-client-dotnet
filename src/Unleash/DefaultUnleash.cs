using System;

namespace Unleash
{
    using Logging;
    using Strategies;
    using System.Collections.Generic;
    using Internal;
    using System.Linq;

    /// <inheritdoc />
    public class DefaultUnleash : IUnleash
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultUnleash));

        private static readonly UnknownStrategy UnknownStrategy = new UnknownStrategy();

        private static readonly IStrategy[] DefaultStrategies = {
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
        ///// Initializes a new instance of Unleash client with a set of default strategies. 
        ///// </summary>
        ///// <param name="config">Unleash settings</param>
        ///// <param name="strategies">Additional custom strategies.</param>
        public DefaultUnleash(UnleashSettings settings, params IStrategy[] strategies)
            : this(settings, overrideDefaultStrategies: false, strategies)
        { }

        ///// <summary>
        ///// Initializes a new instance of Unleash client.
        ///// </summary>
        ///// <param name="config">Unleash settings</param>
        ///// <param name="overrideDefaultStrategies">When true, it overrides the default strategies.</param>
        ///// <param name="strategies">Custom strategies.</param>
        public DefaultUnleash(UnleashSettings settings, bool overrideDefaultStrategies, params IStrategy[] strategies)
        {
            var settingsValidator = new UnleashSettingsValidator();
            settingsValidator.Validate(settings);

            strategies = SelectStrategies(strategies, overrideDefaultStrategies);
            strategyMap = BuildStrategyMap(strategies);

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
            var featureToggle = GetToggle(toggleName);

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

        public Variant GetVariant(string toggleName)
        {
            var variants = GetVariants(toggleName)?.ToList();
            if (variants == null) return null;
            
            var weights = variants.Select(v => v.Weight).ToArray();
            var position = GetWeightedPosition(weights);

            if (position.HasValue)
            {
                return variants[position.Value];
            }

            return null;
        }

        private int? GetWeightedPosition(int[] weights)
        {
            //TODO: this weighted algorithm assumes that the variants list will ever return in same order 
            var total = weights.Sum();
            //TODO: better random generator
            var random = new Random(Guid.NewGuid().GetHashCode()).Next(total);
            var currentWeight = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (random <= currentWeight)
                {
                    return i;
                }
            }

            return null;
        }

        public IEnumerable<Variant> GetVariants(string toggleName)
        {
            if (!IsEnabled(toggleName)) return null;
            
            var toggle = GetToggle(toggleName);
            
            return toggle?.Variants;
        }

        public IEnumerable<Variant> GetVariants(string toggleName, string variantName)
        {
            var variants = GetVariants(toggleName)?.ToList();
            if (variants == null) return null;
            
            variants = variants.Where(v => v.Name == variantName).ToList();

            if (variants.Count == 0) return null;

            return variants;
        }

        private FeatureToggle GetToggle(string toggleName)
        {
            return services
                .ToggleCollection
                .Instance
                .GetToggleByName(toggleName);
        }

        private void RegisterCount(string toggleName, bool enabled)
        {
            if (services.IsMetricsDisabled)
                return;

            services.MetricsBucket.RegisterCount(toggleName, enabled);
        }

        private static IStrategy[] SelectStrategies(IStrategy[] strategies, bool overrideDefaultStrategies)
        {
            if (overrideDefaultStrategies)
            {
                return strategies ?? Array.Empty<IStrategy>();
            }
            else
            {
                return DefaultStrategies.Concat(strategies).ToArray();
            }
        }

        private static Dictionary<string, IStrategy> BuildStrategyMap(IStrategy[] strategies)
        {
            var map = new Dictionary<string, IStrategy>(strategies.Length);

            foreach (var strategy in strategies)
                map.Add(strategy.Name, strategy);

            return map;
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