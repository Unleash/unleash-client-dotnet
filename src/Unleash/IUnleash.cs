using System.Collections.Generic;
using Unleash.Internal;

namespace Unleash
{
    /// <inheritdoc />
    /// <summary>
    /// Unleash Feature Toggle Service
    /// </summary>
    public interface IUnleash
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
        /// Gets a list of a given variant from a feature that is available.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <param name="variantName">The name of the variant</param>
        /// <returns>A list of variants of a given name or null if feature is not available</returns>
        IEnumerable<Variant> GetVariants(string toggleName, string variantName);

        /// <summary>
        ///  Gets a list of variants from a feature that is available.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <returns>A list of variants or null if feature is not available </returns>
        IEnumerable<Variant> GetVariants(string toggleName);

        /// <summary>
        /// Get a weighted variant from a feature that is available.
        /// Should be used with care, is not a sticky variant, will weight by call.
        /// </summary>
        /// <param name="toggleName">The name of the toggle</param>
        /// <returns>A weighted variant or null if feature is not available</returns>
        Variant GetVariant(string toggleName);
    }
}
