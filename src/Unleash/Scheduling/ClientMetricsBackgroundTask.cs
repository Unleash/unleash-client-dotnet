using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Unleash.Metrics;

namespace Unleash.Scheduling
{
    internal class ClientMetricsBackgroundTask : IUnleashScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientMetricsBackgroundTask));

        private readonly IUnleashApiClient apiClient;
        private readonly UnleashSettings settings;
        private readonly MetricsBucket metricsBucket;

        public ClientMetricsBackgroundTask(IUnleashApiClient apiClient, UnleashSettings settings, MetricsBucket metricsBucket)
        {
            this.apiClient = apiClient;
            this.settings = settings;
            this.metricsBucket = metricsBucket;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (settings.SendMetricsInterval == null)
                return;

            if (metricsBucket.Toggles.Count == 0)
            {
                metricsBucket.Clear();
                return;
            }

            var metrics = new ClientMetrics
            {
                AppName = settings.AppName,
                InstanceId = settings.InstanceTag,
                Bucket = metricsBucket
            };

            try
            {
                metricsBucket.End();

                var result = await apiClient.SendMetrics(metrics, cancellationToken).ConfigureAwait(false);

                // Ignore return value    
                if (!result)
                {
                    // Logged elsewhere.
                }
            }
            finally
            {
                metricsBucket.Clear();
            }
        }

        public string Name => "report-metrics-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}