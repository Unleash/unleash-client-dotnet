using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using Unleash.Metrics;
using Unleash.Repository;
using Unleash.Serialization;
using Unleash.Util;

namespace Unleash
{
    internal class UnleashConfigServices
    {
        internal IFileSystem FileSystem { get; set; }
        internal IJsonSerializer JsonSerializer { get; set; }
        internal MetricsBucket MetricsBucket { get; set; }
        internal HttpClient HttpClient { get; set; }
        internal readonly CancellationTokenSource CancellationTokenSource;
        internal readonly CancellationToken CancellationToken;

        public UnleashConfigServices()
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
        }
    }

    public class UnleashConfig
    {
        private const int ApiVersion = 1;
        private bool isInitialized;

        internal readonly Encoding Encoding = Encoding.UTF8;
        internal UnleashConfigServices Services { get; }

        public UnleashConfig()
        {
            Services = new UnleashConfigServices()
            {
            };

            CustomHttpHeaders = new Dictionary<string, string>();
            FetchTogglesInterval = TimeSpan.FromSeconds(20);
            SendMetricsInterval = TimeSpan.FromSeconds(60);
            IsMetricsDisabled = true;
        }

        private static readonly List<IDynamicJsonSerializer> DynamicJsonSerializers = new List<IDynamicJsonSerializer>()
        {
            new NewtonsoftByDynamicJsonSerializer(),
        };

        internal void ValidateUserInputAndSetDefaults()
        {
            if (UnleashApi == null)
                throw new UnleashException("You are required to specify the unleashAPI Uri");

            if (AppName == null)
                throw new UnleashException("You are required to specify the unleash appName");

            if (Services.JsonSerializer == null)
            {
                foreach (var jsonSerializer in DynamicJsonSerializers)
                {
                    if (!jsonSerializer.TryLoad())
                        continue;

                    Services.JsonSerializer = jsonSerializer;
                    break;
                }

                // None?
                if (Services.JsonSerializer == null)
                {
                    var serializers = string.Join(", ", Enumerable.Select<IDynamicJsonSerializer, string>(DynamicJsonSerializers, x => x.NugetPackageName));
                    throw new UnleashException($"Tried to load '{serializers}' json library(ies) but could not find any.{Environment.NewLine}Please add a reference to one of these nuget packages, or implement the '{nameof(IJsonSerializer)}' interface with your favorite json library. This needs to be wired up through the bootstrapping config through {nameof(SetJsonSerializer)}.");
                }
            }

            SdkVersion = GetDefaultSdkVersion();
            InstanceId = InstanceId ?? GetDefaultInstanceId();
            SdkVersion = GetDefaultSdkVersion();

            ContextProvider = ContextProvider ?? new DefaultUnleashContextProvider();
            BackupFile = BackupFile ?? GetDefaultBackupFile();

            // Services
            var httpClientFactory = new HttpClientFactory(this);
            Services.HttpClient = httpClientFactory.Create();
            
            Services.FileSystem = Services.FileSystem ?? new FileSystem(this);
            Services.MetricsBucket = new MetricsBucket();

            // Ensure files exists.
            if (!Services.FileSystem.FileExists(BackupFile))
                Services.FileSystem.WriteAllText(BackupFile, string.Empty);

            if (!Services.FileSystem.FileExists(BackupEtagFile))
                Services.FileSystem.WriteAllText(BackupEtagFile, string.Empty);

            isInitialized = true;
        }


        private Uri unleashApi;

        /// <summary>
        /// Sets the Uri for the unleash api server. (E.g. http://unleash.herokuapp.com/)
        /// </summary>
        public Uri UnleashApi
        {
            get => unleashApi;
            set
            {
                if (isInitialized)
                    throw new UnleashException("Cannot change the api uri after unleash is instantiated.");

                unleashApi = value;
            }
        }

        /// <summary>
        /// Sets the Uri for the unleash api server. (E.g. http://unleash.herokuapp.com/)
        /// </summary>
        public UnleashConfig SetUnleashApi(Uri unleashApi)
        {
            UnleashApi = unleashApi;
            return this;
        }

        /// <summary>
        /// Sets the Uri for the unleash api server. (E.g. http://unleash.herokuapp.com/)
        /// </summary>
        public UnleashConfig SetUnleashApi(string unleashApi)
        {
            UnleashApi = new Uri(unleashApi);
            return this;
        }

        public string AppName { get; set; }

        public UnleashConfig SetAppName(string appName)
        {
            AppName = appName;
            return this;
        }

        public string InstanceId { get; set; }
        public string SdkVersion { get; private set; }
        public TimeSpan FetchTogglesInterval { get; set; }
        public TimeSpan SendMetricsInterval { get; set; }
        public bool IsMetricsDisabled { get; set; }

        public string BackupFile { get; set; }
        internal string BackupEtagFile => BackupFile + ".etag.txt";

        internal bool DisableBackgroundTasks { get; set; }
        internal ToggleCollection InMemoryTogglesForUnitTestingPurposes { get; set; }

        public Dictionary<string, string> CustomHttpHeaders { get; set; }
        public IUnleashContextProvider ContextProvider { get; set; }

        public UnleashConfig AddCustomHttpHeader(string name, string value)
        {
            if (CustomHttpHeaders.ContainsKey(name))
            {
                var existingValue = CustomHttpHeaders[name];
                if (existingValue == value)
                    return this;

                throw new UnleashException($"Given key '{name}' has already been added with value '{existingValue}'. You tried to enter '{value}'.");
            }

            CustomHttpHeaders.Add(name, value);
            return this;
        }

        public UnleashConfig SetInstanceId(string instanceId)
        {
            InstanceId = instanceId;
            return this;
        }

        public UnleashConfig SetJsonSerializer(IJsonSerializer serializer)
        {
            Services.JsonSerializer = serializer;
            return this;
        }

        public UnleashConfig SetFetchTogglesInterval(TimeSpan fetchTogglesInterval)
        {
            FetchTogglesInterval = fetchTogglesInterval;
            return this;
        }

        public UnleashConfig SetSendMetricsInterval(TimeSpan metricsInterval)
        {
            this.SendMetricsInterval = metricsInterval;
            return this;
        }

        public UnleashConfig DisableMetrics()
        {
            IsMetricsDisabled = true;
            return this;
        }

        public UnleashConfig EnableMetrics()
        {
            IsMetricsDisabled = false;
            return this;
        }

        public UnleashConfig SetBackupFile(string backupFile)
        {
            BackupFile = backupFile;
            return this;
        }

        internal UnleashConfig SetFileSystem(IFileSystem fileSystem)
        {
            Services.FileSystem = fileSystem;
            return this;
        }

        internal UnleashConfig SetBackgroundTasksDisabled()
        {
            DisableBackgroundTasks = true;
            return this;
        }

        public UnleashConfig UnleashContextProvider(IUnleashContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
            return this;
        }

        private static string GetDefaultInstanceId()
        {
            var hostName = string.Empty;

            try
            {
                hostName = UnleashExtensions.GetLocalIpAddress() + "-";
            }
            catch (Exception)
            {
            }

            return hostName + "generated-" + Math.Round(new Random(DateTime.Now.Millisecond).Next() * 1000000.0D);
        }

        private string GetDefaultBackupFile()
        {
            var fileName = $"unleash-{AppName}-cache-v{ApiVersion}.json";
            return Path.Combine(Path.GetTempPath(), fileName);
        }

        private static string GetDefaultSdkVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return "unleash-dotnet-client: " + version;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"AppName: {AppName}");
            sb.AppendLine($"Server Uri: {UnleashApi}");
            sb.AppendLine($"SdkVersion: {SdkVersion}");
            sb.AppendLine("...");

            return sb.ToString();
        }
    }
}