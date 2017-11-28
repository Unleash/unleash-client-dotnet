using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Repository;
using Unleash.Util;

namespace Unleash.Metrics
{
    internal class ClientMetricsBackgroundTask : IBackgroundTask
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

        public async Task Execute(CancellationToken cancellationToken)
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
    }

    //internal class ClientMetricsBackgroundTask : IBackgroundTask
    //{
    //    private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientMetricsBackgroundTask));

    //    private readonly UnleashConfig config;

    //    public ClientMetricsBackgroundTask(UnleashConfig config)
    //    {
    //        this.config = config;
    //    }

    //    public async Task Execute(CancellationToken cancellationToken)
    //    {
    //        if (config.IsMetricsDisabled)
    //            return;

    //        var metricsBucket = config.Services.MetricsBucket;
    //        if (metricsBucket.Toggles.Count == 0)
    //            return;

    //        var metrics = new ClientMetrics
    //        {
    //            AppName = config.AppName,
    //            InstanceId = config.InstanceId,
    //            Bucket = metricsBucket
    //        };

    //        using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/client/metrics"))
    //        {
    //            metricsBucket.End();
    //            var stream = config.Services.JsonSerializer.Serialize(metrics);

    //            var content = new StreamContent(stream, 1024 * 4);
    //            content.Headers.TryAddWithoutValidation("Content-Type", "application/json");

    //            httpRequestMessage.Content = content;
    //            httpRequestMessage.SetRequestProperties(config);

    //            using (var response = await config.Services.HttpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
    //            {
    //                if (response.IsSuccessStatusCode)
    //                {
    //                    metricsBucket.Clear();
    //                    return;
    //                }

    //                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    //                Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(ClientMetricsBackgroundTask)}': " + error);
    //            }
    //        }
    //    }
    //}
}