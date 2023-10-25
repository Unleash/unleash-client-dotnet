using System;
using System.IO;
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
        private readonly UnleashEngine engine;

        public ClientMetricsBackgroundTask(
            IUnleashApiClient apiClient, 
            UnleashSettings settings,
            UnleashEngine engine)
        {
            this.apiClient = apiClient;
            this.settings = settings;
            this.engine = engine;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (settings.SendMetricsInterval == null)
                return;

            var metricsBucket = engine.GetMetrics();

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