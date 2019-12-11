using System;

namespace Unleash
{
    using Logging;
    using Strategies;
    using System.Collections.Generic;
    using Internal;
    using System.Linq;

    /// <inheritdoc />
    public class Unleash : IUnleash
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(Unleash));

        private static readonly UnknownStrategy UnknownStrategy = new UnknownStrategy();

        private readonly UnleashSettings settings;
        private readonly IUnleashServices services;
        private readonly IUnleashContextProvider contextProvider;

        ///// <summary>
        ///// Initializes a new instance of Unleash client.
        ///// </summary>
        public Unleash(UnleashSettings settings, IUnleashServices services, IUnleashContextProvider contextProvider)
        {
            this.settings = settings;
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));

            Logger.Info($"UNLEASH: Unleash is initialized and configured with: {settings}");
        }

        public bool IsEnabled(string toggleName)
        {
            return IsEnabled(toggleName, false);
        }

        public bool IsEnabled(string toggleName, bool defaultSetting)
        {
            return CheckIsEnabled(toggleName, contextProvider.Context, defaultSetting);
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
            var random = this.services.Random.Next(total);
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
                .GetToggleCollection()
                .GetToggleByName(toggleName);
        }

        private void RegisterCount(string toggleName, bool enabled)
        {
            var isMetricsDisabled = settings.SendMetricsInterval == null;
            if (isMetricsDisabled)
                return;

            services.RegisterCount(toggleName, enabled);
        }

        private IStrategy GetStrategyOrUnknown(string strategy)
        {
            return services.StrategyMap.TryGetValue(strategy, out var result)
                ? result
                : UnknownStrategy;
        }
    }
}
