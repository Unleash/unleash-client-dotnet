using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using Unleash.Internal;

namespace Unleash
{
    /// <summary>
    /// Unleash settings
    /// </summary>
    public class UnleashSettings
    {
        // TODO: Remove these and/or move closer to FileSystemToggleCollectionCache
        internal readonly string FeatureToggleFilename = "unleash.toggles.json";
        internal readonly string EtagFilename = "unleash.etag.txt";

        /// <summary>
        /// Gets or set the uri for the backend unleash server.
        /// </summary>
        [Required]
        public Uri UnleashApi { get; set; }

        /// <summary>
        /// Gets or sets an application name. Used for communication with backend api.
        /// </summary>
        [Required]
        public string AppName { get; set; }

        /// <summary>
        /// Gets or sets an instance tag. Used for communication with backend api.
        /// </summary>
        [Required]
        public string InstanceTag { get; set; }

        /// <summary>
        /// Gets or sets the interval in which feature toggle changes are re-fetched.
        ///
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan FetchTogglesInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets whether the client will be registered with the unleash server.
        /// </summary>
        public bool RegisterClient { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval in which metrics are sent to the server. When null, no metrics is sent.
        ///
        /// Default: null
        /// </summary>
        public TimeSpan? SendMetricsInterval { get; set; } = null;

        /// <summary>
        /// Gets or set a directory for storing temporary files (toggles and current etag values).
        ///
        /// Default: Path.GetTempPath()
        /// </summary>
        // TODO: (Breaking) Move this to an FileSystemToggleCollectionCacheSettings class, follow pattern
        public string LocalStorageFolder { get; set; } = Path.GetTempPath();

        /// <summary>
        /// Gets or sets a collection of custom http headers which will be included when communicating with the backend server.
        /// </summary>
        /// <remarks>
        /// HttpClientFactoryApiClientFactory should be configured via builder => { builder.ConfigureHttpClient((sp, httpClient) => {}); }
        /// parameter on services.AddUnleash().WithHttpClientFactory(...)
        /// </remarks>
        // TODO: (Breaking) Move this to a settings class closer to DefaultUnleashApiClientFactory.
        public Dictionary<string, string> CustomHttpHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Returns info about the unleash setup.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder("## Unleash settings ##");

            sb.AppendLine($"Application name: {AppName}");
            sb.AppendLine($"Instance tag: {InstanceTag}");
            sb.AppendLine($"Server Uri: {UnleashApi}");
            sb.AppendLine($"Sdk version: {SdkVersionHelper.SdkVersion}");

            sb.AppendLine($"Fetch toggles interval: {FetchTogglesInterval.TotalSeconds} seconds");
            var metricsInterval = SendMetricsInterval.HasValue
                ? $"{SendMetricsInterval.Value.TotalSeconds} seconds"
                : "never";
            sb.AppendLine($"Send metrics interval: {metricsInterval}");

            sb.AppendLine($"Local storage folder: {LocalStorageFolder}");
            sb.AppendLine($"Backup file: {FeatureToggleFilename}");
            sb.AppendLine($"Etag file: {EtagFilename}");

            return sb.ToString();
        }
    }
}
