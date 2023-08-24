namespace Unleash
{
    using Internal;
    using Logging;
    using Strategies;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Unleash.Events;
    using Unleash.Variants;

    /// <inheritdoc />
    public class DefaultUnleash : IUnleash
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultUnleash));

        private static readonly UnknownStrategy UnknownStrategy = new UnknownStrategy();

        private static int InitializedInstanceCount = 0;

        private const int WarnOnInstanceCount = 10;

        private static readonly IStrategy[] DefaultStrategies = {
            new DefaultStrategy(),
            new UserWithIdStrategy(),
            new GradualRolloutUserIdStrategy(),
            new GradualRolloutRandomStrategy(),
            new ApplicationHostnameStrategy(),
            new GradualRolloutSessionIdStrategy(),
            new RemoteAddressStrategy(),
            new FlexibleRolloutStrategy()
        };

        private readonly UnleashSettings settings;

        private readonly Dictionary<string, IStrategy> strategyMap;

        internal readonly UnleashServices services;

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
            var currentInstanceNo = Interlocked.Increment(ref InitializedInstanceCount);

            this.settings = settings;

            var settingsValidator = new UnleashSettingsValidator();
            settingsValidator.Validate(settings);

            strategies = SelectStrategies(strategies, overrideDefaultStrategies);
            strategyMap = BuildStrategyMap(strategies);

            services = new UnleashServices(settings, EventConfig, strategyMap);

            Logger.Info($"UNLEASH: Unleash instance number { currentInstanceNo } is initialized and configured with: {settings}");

            if (currentInstanceNo >= WarnOnInstanceCount)
            {
                Logger.Warn($"UNLEASH: Unleash instance count for this process is now {currentInstanceNo}.");
                Logger.Warn("Ideally you should only need 1 instance of Unleash per app/process, we strongly recommend setting up Unleash as a singleton.");
            }
        }

        /// <inheritdoc />
        public ICollection<FeatureToggle> FeatureToggles => services.ToggleCollection.Instance.Features;

        private EventCallbackConfig EventConfig { get; } = new EventCallbackConfig();

        /// <inheritdoc />
        public bool IsEnabled(string toggleName)
        {
            return IsEnabled(toggleName, false);
        }

        /// <inheritdoc />
        public bool IsEnabled(string toggleName, bool defaultSetting)
        {
            return IsEnabled(toggleName, services.ContextProvider.Context, defaultSetting);
        }

        public bool IsEnabled(string toggleName, UnleashContext context)
        {
            return IsEnabled(toggleName, context, false);
        }

        public bool IsEnabled(string toggleName, UnleashContext context, bool defaultSetting)
        {
            return CheckIsEnabled(toggleName, context, defaultSetting).Enabled;
        }

        private FeatureEvaluationResult CheckIsEnabled(
            string toggleName,
            UnleashContext context,
            bool defaultSetting,
            Variant defaultVariant = null)
        {
            var featureToggle = GetToggle(toggleName);
            var enhancedContext = context.ApplyStaticFields(settings);
            var enabled = DetermineIsEnabledAndStrategy(featureToggle, enhancedContext, defaultSetting, out var strategy);
            var variant = DetermineVariant(enabled, featureToggle, strategy, enhancedContext, defaultVariant);

            RegisterCount(toggleName, enabled);

            if (featureToggle?.ImpressionData ?? false)
                EmitImpressionEvent("isEnabled", enhancedContext, enabled, featureToggle.Name);

            return new FeatureEvaluationResult { Enabled = enabled, Variant = variant };
        }

        private bool DetermineIsEnabledAndStrategy(
            FeatureToggle featureToggle,
            UnleashContext enhancedContext,
            bool defaultSetting,
            out ActivationStrategy strategy)
        {
            strategy = null;
            if (featureToggle == null)
                return defaultSetting;

            else if (!featureToggle.Enabled)
                // Overall false
                return false;

            else if (featureToggle.Strategies.Count == 0)
                return true;

            else
            {
                strategy = featureToggle.Strategies
                    .FirstOrDefault(s =>
                        GetStrategyOrUnknown(s.Name)
                        .IsEnabled(s.Parameters, enhancedContext, ResolveConstraints(s).Union(s.Constraints))
                    );

                return strategy != null;
            }
        }

        private Variant DetermineVariant(bool enabled,
            FeatureToggle featureToggle,
            ActivationStrategy strategy,
            UnleashContext context,
            Variant defaultVariant)
        {
            if (enabled)
            {
                if (strategy != null)
                {
                    strategy.Parameters.TryGetValue("groupId", out string groupId);
                    groupId = groupId ?? featureToggle.Name;
                    return VariantUtils.SelectVariant(groupId, context, strategy.Variants);
                }

                return VariantUtils.SelectVariant(featureToggle, context, defaultVariant);
            }
            else
                return defaultVariant;
        }

        public Variant GetVariant(string toggleName)
        {
            return GetVariant(toggleName, services.ContextProvider.Context, Variant.DISABLED_VARIANT);
        }

        public Variant GetVariant(string toggleName, Variant defaultVariant)
        {
            return GetVariant(toggleName, services.ContextProvider.Context, defaultVariant);
        }

        public Variant GetVariant(string toggleName, UnleashContext context, Variant defaultValue)
        {
            var toggle = GetToggle(toggleName);

            var evaluationResult = CheckIsEnabled(toggleName, context, false, defaultValue);

            RegisterVariant(toggleName, evaluationResult.Variant);

            var enhancedContext = context.ApplyStaticFields(settings);

            if (toggle?.ImpressionData ?? false)
                EmitImpressionEvent("getVariant", enhancedContext, evaluationResult.Enabled, toggle.Name, evaluationResult.Variant?.Name);

            return evaluationResult.Variant;
        }

        public IEnumerable<VariantDefinition> GetVariants(string toggleName)
        {
            return GetVariants(toggleName, services.ContextProvider.Context);
        }

        public IEnumerable<VariantDefinition> GetVariants(string toggleName, UnleashContext context)
        {
            if (!IsEnabled(toggleName, context)) return null;

            var toggle = GetToggle(toggleName);

            return toggle?.Variants;
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

        private void RegisterVariant(string toggleName, Variant variant)
        {
            if (services.IsMetricsDisabled)
                return;

            services.MetricsBucket.RegisterCount(toggleName, variant.Name);
        }

        private static IStrategy[] SelectStrategies(IStrategy[] strategies, bool overrideDefaultStrategies)
        {
            if (overrideDefaultStrategies)
            {
                return strategies ?? new IStrategy[0];
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

        private IEnumerable<Constraint> ResolveConstraints(ActivationStrategy activationStrategy)
        {
            foreach (var segmentId in activationStrategy.Segments)
            {
                var segment = services.ToggleCollection.Instance.GetSegmentById(segmentId);
                if (segment != null)
                {
                    foreach (var constraint in segment.Constraints)
                    {
                        yield return constraint;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        public void ConfigureEvents(Action<EventCallbackConfig> callback)
        {
            if (callback == null)
            {
                Logger.Error($"UNLEASH: Unleash->ConfigureEvents parameter callback is null");
                return;
            }

            try
            {
                callback(EventConfig);
            }
            catch (Exception ex)
            {
                Logger.Error($"UNLEASH: Unleash->ConfigureEvents executing callback threw exception: {ex.Message}");
            }
        }

        private void EmitImpressionEvent(string type, UnleashContext context, bool enabled, string name, string variant = null)
        {
            if (EventConfig?.ImpressionEvent == null)
            {
                Logger.Error($"UNLEASH: Unleash->ImpressionData callback is null, unable to emit event");
                return;
            }

            try
            {
                EventConfig.ImpressionEvent(new ImpressionEvent
                {
                    Type = type,
                    Context = context,
                    EventId = Guid.NewGuid().ToString(),
                    Enabled = enabled,
                    FeatureName = name,
                    Variant = variant
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"UNLEASH: Emitting impression event callback threw exception: {ex.Message}");
            }
        }

        public void Dispose()
        {
            services?.Dispose();
        }
    }
}