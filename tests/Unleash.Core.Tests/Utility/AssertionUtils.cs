using System.Collections.Generic;
using System.Linq;
using Unleash.Internal;
using Xunit;

namespace Unleash.Core.Tests.Utility
{
    public static class AssertionUtils
    {
        public static void AssertToggleCollectionsEquivalent(ToggleCollection expectedToggleCollection, ToggleCollection actualToggleCollection)
        {
            Assert.Equal(expectedToggleCollection.Version, actualToggleCollection.Version);

            AssertFeaturesEquivalent(expectedToggleCollection.Features, actualToggleCollection.Features);
        }

        public static void AssertFeaturesEquivalent(IEnumerable<FeatureToggle> expectedFeatures, IEnumerable<FeatureToggle> actualFeatures)
        {
            Assert.All(
                expectedFeatures.Zip(actualFeatures, (expectedFeature, actualFeature) => (expectedFeature, actualFeature)),
                pair =>
                {
                    var expectedFeature = pair.expectedFeature;
                    var actualFeature = pair.actualFeature;

                    Assert.Equal(expectedFeature.Enabled, actualFeature.Enabled);
                    Assert.Equal(expectedFeature.Name, actualFeature.Name);

                    AssertStrategiesEquivalent(expectedFeature.Strategies, actualFeature.Strategies);
                    AssertVariantsEquivalent(expectedFeature.Variants, actualFeature.Variants);
                });
        }

        public static void AssertVariantsEquivalent(IEnumerable<Variant> expectedVariants, IEnumerable<Variant> actualVariants)
        {
            Assert.All(
                expectedVariants.Zip(actualVariants, (expectedVariant, actualVariant) => (expectedVariant, actualVariant)),
                variantPair =>
                {
                    var expectedVariant = variantPair.expectedVariant;
                    var actualVariant = variantPair.actualVariant;

                    Assert.Equal(expectedVariant.Name, actualVariant.Name);
                    Assert.Equal(expectedVariant.Weight, actualVariant.Weight);

                    if (expectedVariant.Payload != null && actualVariant.Payload != null)
                    {
                        Assert.Equal(expectedVariant.Payload.Type, actualVariant.Payload.Type);
                        Assert.Equal(expectedVariant.Payload.Value, actualVariant.Payload.Value);
                    }
                    else
                    {
                        Assert.Equal(expectedVariant.Payload != null, actualVariant.Payload != null);
                    }

                    if (expectedVariant.Overrides != null && actualVariant.Overrides != null)
                    {
                        AssertVariantOverridesEquivalent(expectedVariant.Overrides, actualVariant.Overrides);
                    }
                    else
                    {
                        Assert.Equal(expectedVariant.Overrides != null, actualVariant.Overrides != null);
                    }
                });
        }

        public static void AssertVariantOverridesEquivalent(IEnumerable<Override> expectedVariantOverrides, IEnumerable<Override> actualVariantOverrides)
        {
            Assert.All(
                expectedVariantOverrides.Zip(actualVariantOverrides, (expectedOverride, actualOverride) => (expectedOverride, actualOverride)),
                overridePair =>
                {
                    var expectedOverride = overridePair.expectedOverride;
                    var actualOverride = overridePair.actualOverride;

                    Assert.Equal(expectedOverride.Values, actualOverride.Values);
                    Assert.Equal(expectedOverride.ContextName, actualOverride.ContextName);
                }
            );
        }

        public static void AssertStrategiesEquivalent(IEnumerable<ActivationStrategy> expectedStrategies, IEnumerable<ActivationStrategy> actualStrategies)
        {
            Assert.All(
                expectedStrategies.Zip(actualStrategies, (expectedStrategy, actualStrategy) => (expectedStrategy, actualStrategy)),
                strategyPair =>
                {
                    var expectedStrategy = strategyPair.expectedStrategy;
                    var actualStrategy = strategyPair.actualStrategy;

                    Assert.Equal(expectedStrategy.Name, actualStrategy.Name);
                    Assert.All(expectedStrategy.Parameters, kvp => Assert.Equal(actualStrategy.Parameters[kvp.Key], kvp.Value));
                });
        }
    }
}
