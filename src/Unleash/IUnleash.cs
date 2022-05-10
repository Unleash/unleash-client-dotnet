using System;
using System.Collections.Generic;
using Unleash.Internal;
using Unleash.Variants;

namespace Unleash
{
    /// <inheritdoc />
    /// <summary>
    /// Unleash Feature Toggle Service
    /// </summary>
    public interface IUnleash : IDisposable
    {
        /// <summary>
        /// Collection of currently loaded Feature Toggles
        /// </summary>
        ICollection<FeatureToggle> FeatureToggles { get; }

        /// <summary>
        /// Determines if the given feature toggle is enabled or not, defaulting to <c>false</c> if the toggle cannot be found.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        bool IsEnabled(string toggleName);

        /// <summary>
        /// Determines if the given feature toggle is enabled or not, defaulting to the value of <paramref name="defaultSetting"/> if the toggle cannot be found.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="defaultSetting">Default value to return if toggle is not defined</param>
        bool IsEnabled(string toggleName, bool defaultSetting);

        /// <summary>
        /// Determines if the given feature toggle is enabled or not, defaulting to <c>false</c> if the toggle cannot be found.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="context">The Unleash context to evaluate the toggle state against</param>
        bool IsEnabled(string toggleName, UnleashContext context);

        /// <summary>
        /// Determines if the given feature toggle is enabled or not, defaulting to the value of <paramref name="defaultSetting"/> if the toggle cannot be found.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="context">The Unleash context to evaluate the toggle state against</param>
        /// <param name="defaultSetting">Default value to return if toggle is not defined</param>
        bool IsEnabled(string toggleName, UnleashContext context, bool defaultSetting);

        /// <summary>
        /// Get a weighted variant from a feature that is available.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <returns>A weighted variant or Variant.DISABLED_VARIANT if feature is not available</returns>
        Variant GetVariant(string toggleName);

        /// <summary>
        /// Get a weighted variant from a feature that is available.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="defaultValue">If a toggle is not found, the default value will be returned.</param>
        /// <returns>A weighted variant or the supplied default value if feature is not available</returns>
        Variant GetVariant(string toggleName, Variant defaultValue);

        /// <summary>
        /// Get a weighted variant from a feature that is available.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="context">The Unleash context to evaluate the toggle state against</param>
        /// <param name="defaultValue">If a toggle is not found, the default value will be returned.</param>
        /// <returns>A weighted variant or the supplied default value if feature is not available</returns>
        Variant GetVariant(string toggleName, UnleashContext context, Variant defaultValue);

        /// <summary>
        /// Get available feature variants.
        /// </summary>
        /// <param name="toggleName">The name of the toggle.</param>
        /// <returns>A list of available variants.</returns>
        IEnumerable<VariantDefinition> GetVariants(string toggleName);

        /// <summary>
        /// Get available feature variants.
        /// </summary>
        /// <param name="toggleName">The name of the toggle.</param>
        /// /// <param name="context">The Unleash context to evaluate the toggle state against.</param>
        /// <returns>A list of available variants.</returns>
        IEnumerable<VariantDefinition> GetVariants(string toggleName, UnleashContext context);
    }
}