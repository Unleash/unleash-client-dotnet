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
        /// Gets a value indicating a feature is available or not.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        bool IsEnabled(string toggleName);

        /// <summary>
        /// Gets a value indicating a feature is available or not.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="defaultSetting">If a toggle is not found, default fallback setting will be returned. (default: false)</param>
        /// <returns></returns>
        bool IsEnabled(string toggleName, bool defaultSetting);

        /// <summary>
        /// Get a weighted variant from a feature that is available.
        /// Should be used with care, is not a sticky variant, will weight by call. 
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <returns>A weighted variant or null if feature is not available</returns>
        Variant GetVariant(string toggleName);

        /// <summary>
        /// Get a weighted variant from a feature that is available.
        /// Should be used with care, is not a sticky variant, will weight by call. 
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="defaultValue">If a toglge is not found, the default value will be returned. (Default: Variant.DISABLED_VARIANT)</param>
        /// <returns>A weighted variant or null if feature is not available</returns>
        Variant GetVariant(string toggleName, Variant defaultValue);

        IEnumerable<VariantDefinition> GetVariants(string toggleName);
    }
}