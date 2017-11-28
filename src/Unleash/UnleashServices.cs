using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unleash.Metrics;
using Unleash.Repository;
using Unleash.Strategies;
using Unleash.Util;

namespace Unleash
{
    internal class UnleashServices : IDisposable
    {
        internal CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        internal CancellationToken CancellationToken { get; }

        internal IUnleashApiClient ApiClient { get; set; }
        internal IBackgroundTaskRunner TaskRunner { get; set; }
        internal IFileSystem FileSystem { get; set; }
        internal MetricsBucket MetricsBucket { get; set; }
        internal ToggleCollectionInstance ToggleCollectionInstance { get; set; }

        internal string BackupFile { get; set; }
        internal string EtagBackupFile { get; set; }

        internal IUnleashContextProvider ContextProvider { get; set; }
        public bool IsMetricsDisabled { get; set; }

        public UnleashServices(UnleashSettings settings, Dictionary<string, IStrategy> strategyMap)
        {
            CancellationToken = CancellationTokenSource.Token;
            ContextProvider = settings.UnleashContextProvider;

            FileSystem = new FileSystem(settings.Encoding);

            EnsureFilesAndFolderExists(settings, FileSystem);

            MetricsBucket = new MetricsBucket();

            ApiClient = new UnleashApiClient(settings.UnleashApi, settings.HttpClientFactory, settings.JsonSerializer, new UnleashApiClientRequestHeaders()
            {
                AppName = settings.AppName,
                InstanceId = settings.InstanceTag,
                UserAgent = null,
                CustomHttpHeaders = settings.CustomHttpHeaders
            });

            ToggleCollectionInstance = new ToggleCollectionInstance(
                settings.JsonSerializer, 
                FileSystem, 
                BackupFile);

            TaskRunner = new BackgroundTaskRunnerV2(CancellationToken);

            IsMetricsDisabled = settings.SendMetricsInterval == null;

            var fetchFeatureTogglesTask = new FetchFeatureTogglesTask(
                ApiClient, 
                ToggleCollectionInstance, 
                settings.JsonSerializer, 
                FileSystem, 
                BackupFile, 
                EtagBackupFile);
            TaskRunner.Register(fetchFeatureTogglesTask, settings.FetchTogglesInterval, executeImmediately: true);

            if (settings.SendMetricsInterval != null)
            {
                var clientRegistrationBackgroundTask = new ClientRegistrationBackgroundTask(
                    ApiClient, 
                    settings, 
                    MetricsBucket, 
                    strategyMap.Select(pair => pair.Key).ToList());
                TaskRunner.Register(clientRegistrationBackgroundTask, TimeSpan.Zero, executeImmediately: true);

                var clientMetricsBackgroundTask = new ClientMetricsBackgroundTask(
                    ApiClient, 
                    settings, 
                    MetricsBucket);
                TaskRunner.Register(clientMetricsBackgroundTask, settings.SendMetricsInterval.Value, executeImmediately: false);
            }
        }

        internal void EnsureFilesAndFolderExists(UnleashSettings settings, IFileSystem fileSystem)
        {
            var tempFolder = settings.LocalStorageFolder();

            BackupFile = Path.Combine(tempFolder, PrependFileName(settings, settings.FeatureToggleFilename));
            EtagBackupFile = Path.Combine(tempFolder, PrependFileName(settings, settings.EtagFilename));

            // Ensure files exists.
            if (!fileSystem.FileExists(BackupFile))
                fileSystem.WriteAllText(BackupFile, string.Empty);

            if (!fileSystem.FileExists(EtagBackupFile))
                fileSystem.WriteAllText(EtagBackupFile, string.Empty);
        }

        private string PrependFileName(UnleashSettings settings, string filename)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            var extension = Path.GetExtension(filename);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            return new string($"{fileNameWithoutExtension}-{settings.AppName}-{settings.InstanceTag}-{settings.SdkVersion}{extension}"
                .Where(c => !invalidFileNameChars.Contains(c))
                .ToArray());
        }

        public void Dispose()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            TaskRunner.Dispose();
        }
    }
}