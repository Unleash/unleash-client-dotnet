using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Util;

namespace Unleash.Metrics
{
    internal class ClientRegistrationBackgroundTask : IBackgroundTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientRegistrationBackgroundTask));

        private const string RegisterUri = "api/client/register";

        private readonly UnleashConfig config;
        private readonly List<string> strategies;

        public ClientRegistrationBackgroundTask(UnleashConfig config, List<string> strategies)
        {
            this.config = config;
            this.strategies = strategies;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            if (config.IsMetricsDisabled)
                return;

            var clientRegistration = new ClientRegistration(config, config.Services.MetricsBucket.Start, strategies);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, RegisterUri))
            {
                var stream = config.Services.JsonSerializer.Serialize(clientRegistration);

                var content = new StreamContent(stream, 1024 * 4);
                content.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                httpRequestMessage.Content = content;

                httpRequestMessage.SetRequestProperties(config);

                using (var response = await config.Services.HttpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                        return;

                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(ClientRegistrationBackgroundTask)}': " + error);
                }
            }
        }
    }
}