using System;
using System.Collections.Generic;
using Unleash.Internal;

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

        ICollection<Variants> GetVariants(string toogleName, string name);
    }
}