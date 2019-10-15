using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Unleash
{
    /// <summary>
    /// Unleash settings
    /// </summary>
    public class UnleashSettings
    {
        internal readonly string FeatureToggleFilename = "unleash.toggles.json";
        internal readonly string EtagFilename = "unleash.etag.txt";

        /// <summary>
        /// Gets the version of unleash client running.
        /// </summary>
        public string SdkVersion { get; } = GetSdkVersion();

        /// <summary>
        /// Gets or set the uri for the backend unleash server.
        ///
        /// Default: http://unleash.herokuapp.com/
        /// </summary>
        public Uri UnleashApi { get; set; } = new Uri("http://unleash.herokuapp.com/");

        /// <summary>
        /// Gets or sets an application name. Used for communication with backend api.
        /// </summary>
        public string AppName { get; set; } = "my-awesome-app";

        /// <summary>
        /// Gets or sets an instance tag. Used for communication with backend api.
        /// </summary>
        public string InstanceTag { get; set; } = "Dev";

        /// <summary>
        /// Gets or sets the interval in which feature toggle changes are re-fetched.
        ///
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan FetchTogglesInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the interval in which metriccs are sent to the server. When null, no metrics is sent.
        ///
        /// Default: null
        /// </summary>
        public TimeSpan? SendMetricsInterval { get; set; } = null;

        /// <summary>
        /// Gets or set a directory for storing temporary files (toggles and current etag values).
        ///
        /// Default: Path.GetTempPath()
        /// </summary>
        public Func<string> LocalStorageFolder { get; set; } = Path.GetTempPath;

        /// <summary>
        /// Gets or sets a collection of custom http headers which will be included when communicating with the backend server.
        /// </summary>
        public Dictionary<string, string> CustomHttpHeaders { get; set; } = new Dictionary<string, string>();

        private static string GetSdkVersion()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var version = assemblyName.Version.ToString(3);

            return $"v{version}";
        }

        /// <summary>
        /// Returns info about the unleash setup.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder("## Unleash settings ##");

            sb.AppendLine($"Application name: {AppName}");
            sb.AppendLine($"Instance tag: {InstanceTag}");
            sb.AppendLine($"Server Uri: {UnleashApi}");
            sb.AppendLine($"Sdk version: {SdkVersion}");

            sb.AppendLine($"Fetch toggles interval: {FetchTogglesInterval.TotalSeconds} seconds");
            var metricsInterval = SendMetricsInterval.HasValue
                ? $"{SendMetricsInterval.Value.TotalSeconds} seconds"
                : "never";
            sb.AppendLine($"Send metrics interval: {metricsInterval}");

            sb.AppendLine($"Local storage folder: {LocalStorageFolder()}");
            sb.AppendLine($"Backup file: {FeatureToggleFilename}");
            sb.AppendLine($"Etag file: {EtagFilename}");

            return sb.ToString();
        }

        public string GetFeatureToggleFilePath()
        {
            var tempFolder = LocalStorageFolder();
            return Path.Combine(tempFolder, PrependFileName(FeatureToggleFilename));
        }

        public string GetFeatureToggleETagFilePath()
        {
            var tempFolder = LocalStorageFolder();
            return Path.Combine(tempFolder, PrependFileName(EtagFilename));
        }

        private string PrependFileName(string filename)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            var extension = Path.GetExtension(filename);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            return new string($"{fileNameWithoutExtension}-{AppName}-{InstanceTag}-{SdkVersion}{extension}"
                .Where(c => !invalidFileNameChars.Contains(c))
                .ToArray());
        }
    }
}
