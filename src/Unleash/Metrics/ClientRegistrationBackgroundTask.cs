using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Repository;
using Unleash.Util;

namespace Unleash.Metrics
{
    internal class ClientRegistrationBackgroundTask : IBackgroundTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientRegistrationBackgroundTask));

        private readonly IUnleashApiClient apiClient;
        private readonly UnleashSettings settings;
        private readonly MetricsBucket metricsBucket;
        private readonly List<string> strategies;

        public ClientRegistrationBackgroundTask(IUnleashApiClient apiClient, UnleashSettings settings, MetricsBucket metricsBucket, List<string> strategies)
        {
            this.apiClient = apiClient;
            this.settings = settings;
            this.metricsBucket = metricsBucket;
            this.strategies = strategies;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            if (settings.SendMetricsInterval == null)
                return;

            var clientRegistration = new ClientRegistration
            {
                AppName = settings.AppName,
                InstanceId = settings.InstanceTag,
                Interval = (long)settings.SendMetricsInterval.Value.TotalMilliseconds,
                SdkVersion = settings.SdkVersion,
                Started = metricsBucket.Start,
                Strategies = strategies
            };

            var result = await apiClient.RegisterClient(clientRegistration, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                // Already logged..    
            }
        }
    }

    //internal class ClientRegistrationBackgroundTask : IBackgroundTask
    //{
    //    private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientRegistrationBackgroundTask));

    //    private const string RegisterUri = "api/client/register";

    //    private readonly UnleashConfig config;
    //    private readonly List<string> strategies;

    //    public ClientRegistrationBackgroundTask(UnleashConfig config, List<string> strategies)
    //    {
    //        this.config = config;
    //        this.strategies = strategies;
    //    }

    //    public async Task Execute(CancellationToken cancellationToken)
    //    {
    //        if (config.IsMetricsDisabled)
    //            return;

    //        var clientRegistration = new ClientRegistration()
    //        {
    //            AppName = config.AppName,
    //            InstanceId = config.InstanceId,
    //            Interval = (long) config.SendMetricsInterval.TotalMilliseconds,
    //            SdkVersion = config.SdkVersion,
    //            Started = config.Services.MetricsBucket.Start,
    //            Strategies = strategies
    //        };

    //        using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, RegisterUri))
    //        {
    //            var stream = config.Services.JsonSerializer.Serialize(clientRegistration);

    //            var content = new StreamContent(stream, 1024 * 4);
    //            content.Headers.TryAddWithoutValidation("Content-Type", "application/json");
    //            httpRequestMessage.Content = content;

    //            httpRequestMessage.SetRequestProperties(config);

    //            using (var response = await config.Services.HttpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
    //            {
    //                if (response.IsSuccessStatusCode)
    //                    return;

    //                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    //                Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(ClientRegistrationBackgroundTask)}': " + error);
    //            }
    //        }
    //    }
    //}
}