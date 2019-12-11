using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Unleash.Internal;

namespace Unleash.Lifetime
{
    public class HostControlledLifetimeService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IUnleashServices unleashServices;

        public HostControlledLifetimeService(IServiceProvider serviceProvider, IUnleashServices unleashServices)
        {
            this.serviceProvider = serviceProvider;
            this.unleashServices = unleashServices;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            var logger = serviceProvider.GetService<ILogger<HostControlledLifetimeService>>();

            logger?.LogInformation("Shutting down Unleash...");
            unleashServices.Dispose();
            logger?.LogDebug("Unleash offline.");

            return Task.CompletedTask;
        }
    }
}
