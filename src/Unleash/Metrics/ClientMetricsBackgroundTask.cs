using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Util;

namespace Unleash.Metrics
{
    internal class ClientMetricsBackgroundTask : IBackgroundTask
    {
        private readonly UnleashConfig config;

        public ClientMetricsBackgroundTask(UnleashConfig config)
        {
            this.config = config;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            if (config.IsMetricsDisabled)
                return;

            var metricsBucket = config.Services.MetricsBucket;
            if (metricsBucket.Toggles.Count == 0)
                return;

            var metrics = new ClientMetrics(config, metricsBucket);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/client/metrics"))
            {
                metricsBucket.End();
                var stream = config.Services.JsonSerializer.Serialize(metrics);

                var content = new StreamContent(stream, 1024 * 4);
                content.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                httpRequestMessage.Content = content;
                httpRequestMessage.SetRequestProperties(config);

                using (var response = await config.Services.HttpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        metricsBucket.Clear();
                        return;
                    }

                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }
    }
}