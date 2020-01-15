using System;
using System.Collections.Generic;
using System.Linq;
using Unleash.Internal;
using Unleash.Strategies;

namespace Unleash.Variants
{
    internal class VariantUtils
    {
        public static Variant SelectVariant(FeatureToggle featureToggle, UnleashContext context, Variant defaultVariant)
        {
            var variantDefinitions = featureToggle.Variants;
            var totalWeight = variantDefinitions.Sum(v => v.Weight);

            if (totalWeight == 0) {
                return defaultVariant;
            }

            var variantOverride = GetOverride(variantDefinitions, context);
            if (variantOverride != null)
            {
                return variantOverride.ToVariant();
            }

            var target = StrategyUtils.GetNormalizedNumber(GetIdentifier(context), featureToggle.Name, totalWeight);

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

            return defaultVariant;
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

        private static string GetIdentifier(UnleashContext context)
        {
            return context.UserId
                ?? context.SessionId
                ?? context.RemoteAddress
                ?? new Random().NextDouble().ToString();
        }
    }
}