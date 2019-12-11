using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unleash.Internal;

namespace Unleash.Lifetime
{
    public class SynchronousFlagLoadingService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IUnleashServices unleashServices;
        private readonly SynchronousFlagLoadingServiceOptions synchronousFlagLoadingServiceOptions;

        public SynchronousFlagLoadingService(IServiceProvider serviceProvider, IUnleashServices unleashServices, IOptions<SynchronousFlagLoadingServiceOptions> synchronousFlagLoadingServiceOptions)
        {
            this.serviceProvider = serviceProvider;
            this.unleashServices = unleashServices;
            this.synchronousFlagLoadingServiceOptions = synchronousFlagLoadingServiceOptions.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var logger = serviceProvider.GetService<ILogger<SynchronousFlagLoadingService>>();

            logger?.LogInformation("Waiting to load Feature Flags from Unleash server...");

            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(synchronousFlagLoadingServiceOptions.Timeout);

                unleashServices.FeatureToggleLoadComplete(synchronousFlagLoadingServiceOptions.OnlyOnEmptyCache, cts.Token)
                    .Wait(CancellationToken.None);

                logger?.LogInformation("Unleash Feature Flags loaded successfully.");
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Unleash Feature Flags load cancelled.");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
