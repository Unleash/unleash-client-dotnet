using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Utilities;

namespace Unleash
{
    /// <summary>
    /// Unleash settings
    /// </summary>
    public class UnleashSettings
    {
        internal readonly Encoding Encoding = Encoding.UTF8;

        internal readonly string FeatureToggleFilename = "unleash.toggles.json";
        internal readonly string EtagFilename = "unleash.etag.txt";

        /// <summary>
        /// Gets the version of unleash client running.
        /// </summary>
        public string SdkVersion { get; } = GetSdkVersion();

        /// <summary>
        /// Gets or set the uri for the backend unleash server.
        ///
        /// Default: http://unleash.herokuapp.com/api/
        /// </summary>
        public Uri UnleashApi { get; set; } = new Uri("http://unleash.herokuapp.com/api/");

        /// <summary>
        /// Gets or sets an application name. Used for communication with backend api.
        /// </summary>
        public string AppName { get; set; } = "my-awesome-app";

        /// <summary>
        /// Gets or sets an environment. Used for communication with backend api.
        /// </summary>
        public string Environment { get; set; } = "default";

        /// <summary>
        /// INTERNAL: Gets or sets an instance id. Used for communication with backend api.
        /// </summary>
        internal string InstanceId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Sets the project to fetch feature toggles for.
        /// </summary>
        public string ProjectId { get; set; } = null;

        /// <summary>
        /// Gets or sets the interval in which feature toggle changes are re-fetched.
        ///
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan FetchTogglesInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the interval in which metrics are sent to the server. When null, no metrics are sent.
        ///
        /// Default: 60s
        /// </summary>
        public TimeSpan? SendMetricsInterval { get; set; } = TimeSpan.FromSeconds(60);

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

        /// <summary>
        /// Gets or sets a provider that returns a dictionary of custom http headers
        /// which will be included when communicating with the backend server.
        /// This provider will be called before each outgoing request to the unleash server.
        /// </summary>
        public IUnleashCustomHttpHeaderProvider UnleashCustomHttpHeaderProvider { get; set; } = new DefaultCustomHttpHeaderProvider();

        /// <summary>
        /// Gets or sets the unleash context provider. This is needed when using any of the activation strategies
        /// that needs application specific context like userid etc.
        ///
        /// Default: A provider with no context.
        /// </summary>
        public IUnleashContextProvider UnleashContextProvider { get; set; } = new DefaultUnleashContextProvider();

        /// <summary>
        /// Gets or sets a json serializer.
        ///
        /// Default: A serializer based on Newtonsoft will be used, given that these assemblies are loaded into the appdomain already.
        /// </summary>
        public IJsonSerializer JsonSerializer { get; set; } = new DynamicNewtonsoftJsonSerializer();

        /// <summary>
        /// Get or sets a factory class for creating the HttpClient instance used for communicating with the backend.
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; } = new DefaultHttpClientFactory();

        /// <summary>
        /// Gets or sets the scheduled task manager used for syncing feature toggles and metrics with the backend in the background.
        /// Default: An implementation based on System.Threading.Timers
        /// </summary>
        public IUnleashScheduledTaskManager ScheduledTaskManager { get; set; } = new SystemTimerScheduledTaskManager();


        /// <summary>
        /// INTERNAL: Gets or sets an api client instance. Can be used for testing/mocking etc.
        /// </summary>
        internal IUnleashApiClient UnleashApiClient { get; set; }

        /// <summary>
        /// INTERNAL: Gets or sets the file system abstraction. Can be used for testing/mocking etc.
        /// </summary>
        internal IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets or sets the toggle bootstrap provider (file, url, etc). Can be used for testing/mocking etc.
        /// </summary>
        public IToggleBootstrapProvider ToggleBootstrapProvider { get; set; }

        /// <summary>
        /// Gets or sets the override behaviour of the Bootstrap Toggles feature
        /// </summary>
        public bool BootstrapOverride { get; set; } = true;

        /// <summary>
        /// INTERNAL: Gets or sets if the feature toggle fetch should be immeditely scheduled. Used by the client factory to prevent redundant initial fetches.
        /// </summary>
        internal bool ScheduleFeatureToggleFetchImmediatly { get; set; } = true;

        internal bool ThrowOnInitialFetchFail { get; set; }

        private static string GetSdkVersion()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var version = assemblyName.Version.ToString(3);

            return $"unleash-client-dotnet:v{version}";
        }

        /// <summary>
        /// Returns info about the unleash setup.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder("## Unleash settings ##");

            sb.AppendLine($"Application name: {AppName}");
            sb.AppendLine($"Environment: {Environment}");
            sb.AppendLine($"Instance Id: {InstanceId}");
            sb.AppendLine($"Project Id: {ProjectId}");
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

            sb.AppendLine($"HttpClient Factory: {HttpClientFactory.GetType().Name}");
            sb.AppendLine($"Json serializer: {JsonSerializer.GetType().Name}");
            sb.AppendLine($"Context provider: {UnleashContextProvider.GetType().Name}");

            sb.AppendLine($"Bootstrap overrides: {BootstrapOverride}");
            sb.AppendLine($"Bootstrap provider: {ToggleBootstrapProvider?.GetType().Name ?? "null"}");

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

            return new string($"{fileNameWithoutExtension}-{AppName}-{SdkVersion}{extension}"
                .Where(c => !invalidFileNameChars.Contains(c))
                .ToArray());
        }

        public void UseBootstrapUrlProvider(string path, bool shouldThrowOnError, Dictionary<string, string> customHeaders = null)
        {
            ToggleBootstrapProvider = new ToggleBootstrapUrlProvider(path, HttpClientFactory.Create(new Uri(path)), this, shouldThrowOnError, customHeaders);
        }

        public void UseBootstrapFileProvider(string path)
        {
            ToggleBootstrapProvider = new ToggleBootstrapFileProvider(path, this);
        }
    }
}
