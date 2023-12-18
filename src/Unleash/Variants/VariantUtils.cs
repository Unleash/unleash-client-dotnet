using System;
using System.Collections.Generic;
using System.Linq;
using Unleash.Internal;
using Unleash.Strategies;

namespace Unleash.Variants
{
    internal class VariantUtils
    {
        public static readonly uint VARIANT_NORMALIZATION_SEED = 86028157;

        public static Variant SelectVariant(string groupId, UnleashContext context, List<VariantDefinition> variantDefinitions)
        {
            var stickiness = variantDefinitions.FirstOrDefault()?.Stickiness ?? "default";
            var target = StrategyUtils.GetNormalizedNumber(GetIdentifier(context, stickiness), groupId, VARIANT_NORMALIZATION_SEED);

            var counter = 0;
            Variant result = null;
            foreach (var variantDefinition in variantDefinitions)
            {
                if (variantDefinition.Weight != 0)
                {
                    if (variantDefinition.Overrides.Count > 0 && variantDefinition.Overrides.Any(OverrideMatchesContext(context)))
                    {
                        result = variantDefinition.ToVariant();
                        break;
                    }

                    counter += variantDefinition.Weight;
                    if (counter >= target && result == null)
                    {
                        result = variantDefinition.ToVariant();
                    }
                }
            }

            return result;
        }

        public static Variant SelectVariant(FeatureToggle feature, UnleashContext context, Variant defaultVariant = null)
        {
            if (feature == null)
            {
                return defaultVariant;
            }

            return SelectVariant(feature.Name, context, feature.Variants) ?? defaultVariant;

        }

        private static Func<VariantOverride, bool> OverrideMatchesContext(UnleashContext context)
        {
            return (variantOverride) =>
            {
                string contextValue = null;
                switch (variantOverride.ContextName)
                {
                    case "userId":
                        contextValue = context.UserId;
                        break;
                    case "sessionId":
                        contextValue = context.SessionId;
                        break;
                    case "remoteAddress":
                        contextValue = context.RemoteAddress;
                        break;
                    default:
                        context.Properties.TryGetValue(variantOverride.ContextName, out contextValue);
                        break;
                }
                return variantOverride.Values.Contains(contextValue ?? "");
            };
        }

        private static string GetIdentifier(UnleashContext context, string stickiness)
        {
            if (stickiness != "default")
            {
                var stickinessValue = context.GetByName(stickiness);
                return stickinessValue ?? GetRandomValue();
            }

            return context.UserId
                ?? context.SessionId
                ?? context.RemoteAddress
                ?? GetRandomValue();
        }

        private static string GetRandomValue()
        {
            return new Random().NextDouble().ToString();
        }
    }
}