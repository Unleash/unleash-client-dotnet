using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Unleash.Metrics;

namespace Unleash.Scheduling
{
    internal class ClientRegistrationBackgroundTask : IUnleashScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientRegistrationBackgroundTask));

        private readonly IUnleashApiClient apiClient;
        private readonly UnleashSettings settings;
        private readonly List<string> strategies;

        public ClientRegistrationBackgroundTask(
            IUnleashApiClient apiClient, 
            UnleashSettings settings, 
            List<string> strategies)
        {
            this.apiClient = apiClient;
            this.settings = settings;
            this.strategies = strategies;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (settings.SendMetricsInterval == null)
                return;

            var clientRegistration = new ClientRegistration
            {
                AppName = settings.AppName,
                InstanceId = settings.InstanceTag,
                Interval = (long)settings.SendMetricsInterval.Value.TotalMilliseconds,
                SdkVersion = settings.SdkVersion,
                Started = DateTimeOffset.UtcNow,
                Strategies = strategies
            };

            var result = await apiClient.RegisterClient(clientRegistration, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                // Already logged..    
            }
        }

        public string Name => "register-client-task";

        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}