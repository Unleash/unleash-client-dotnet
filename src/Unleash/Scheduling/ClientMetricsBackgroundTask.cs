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

        private readonly IUnleashApiClientFactory apiClientFactory;
        private readonly UnleashSettings settings;
        private readonly ThreadSafeMetricsBucket metricsBucket;

        public ClientMetricsBackgroundTask(
            IUnleashApiClientFactory apiClientFactory,
            UnleashSettings settings,
            ThreadSafeMetricsBucket metricsBucket)
        {
            this.apiClientFactory = apiClientFactory;
            this.settings = settings;
            this.metricsBucket = metricsBucket;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (settings.SendMetricsInterval == null)
                return;

            var apiClient = apiClientFactory.CreateClient();
            var result = await apiClient.SendMetrics(metricsBucket, cancellationToken).ConfigureAwait(false);

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
