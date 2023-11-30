using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Unleash.Metrics;
using YggdrasilEngine = Yggdrasil.YggdrasilEngine;

namespace Unleash.Scheduling
{
    internal class ClientMetricsBackgroundTask : IUnleashScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientMetricsBackgroundTask));

        private readonly IUnleashApiClient apiClient;
        private readonly UnleashSettings settings;
        private readonly ThreadSafeMetricsBucket metricsBucket;
        private readonly YggdrasilEngine engine;

        public ClientMetricsBackgroundTask(
            IUnleashApiClient apiClient, 
            UnleashSettings settings,
            ThreadSafeMetricsBucket metricsBucket,
            YggdrasilEngine engine)
        {
            this.apiClient = apiClient;
            this.settings = settings;
            this.metricsBucket = metricsBucket;
            this.engine = engine;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (settings.SendMetricsInterval == null)
                return;

            var result = false;

            if (settings.UseYggdrasil)
            {
                var engineBucket = engine.GetMetrics();

                result = await apiClient.SendEngineMetrics(engineBucket, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                result = await apiClient.SendMetrics(metricsBucket, cancellationToken).ConfigureAwait(false);
            }

            // Ignore return value    
            if (!result)
            {
                // Logged elsewhere.
            }
        }

        public string Name => "report-metrics-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}