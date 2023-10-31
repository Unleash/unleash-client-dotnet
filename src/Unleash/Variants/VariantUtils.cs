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
            var totalWeight = variantDefinitions.Sum(v => v.Weight);

            if (totalWeight == 0) {
                return null;
            }

            var variantOverride = GetOverride(variantDefinitions, context);
            if (variantOverride != null)
            {
                return variantOverride.ToVariant();
            }

            var stickiness = variantDefinitions[0].Stickiness ?? "default";
            var target = StrategyUtils.GetNormalizedNumber(GetIdentifier(context, stickiness), groupId, VARIANT_NORMALIZATION_SEED, totalWeight);

            var counter = 0;
            foreach (var variantDefinition in variantDefinitions)
            {
                if (variantDefinition.Weight != 0)
                {
                    counter += variantDefinition.Weight;
                    if (counter >= target)
                    {
                        return variantDefinition.ToVariant();
                    }
                }
            }

            return null;
        }

        public static Variant SelectVariant(FeatureToggle feature, UnleashContext context, Variant defaultVariant = null)
        {
            if (feature == null)
            {
                return defaultVariant;
            }

            return SelectVariant(feature.Name, context, feature.Variants) ?? defaultVariant;

        }

        private static VariantDefinition GetOverride(List<VariantDefinition> variants, UnleashContext context)
        {
            return variants.FirstOrDefault(v => v.Overrides.Any(OverrideMatchesContext(context)));
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