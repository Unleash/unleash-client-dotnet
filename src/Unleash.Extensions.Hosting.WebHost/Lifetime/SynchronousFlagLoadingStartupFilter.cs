using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unleash.Internal;

namespace Unleash.Lifetime
{
    public class SynchronousFlagLoadingStartupFilter : IStartupFilter
    {
        private TimeSpan Timeout { get; }
        private readonly IServiceProvider serviceProvider;
        private readonly IUnleashServices unleashServices;
        private readonly bool onlyOnEmptyCache;

        public SynchronousFlagLoadingStartupFilter(
            IServiceProvider serviceProvider,
            IUnleashServices unleashServices,
            bool onlyOnEmptyCache,
            TimeSpan timeout)
        {
            Timeout = timeout;
            this.serviceProvider = serviceProvider;
            this.unleashServices = unleashServices;
            this.onlyOnEmptyCache = onlyOnEmptyCache;
        }

        /// <inheritdoc />
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            var logger = serviceProvider.GetService<ILogger<SynchronousFlagLoadingStartupFilter>>();
            var lifetime = serviceProvider.GetService<IApplicationLifetime>();

            logger?.LogInformation("Waiting to load Feature Flags from Unleash server...");

            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(Timeout);

                unleashServices.FeatureToggleLoadComplete(onlyOnEmptyCache, cts.Token)
                    .Wait(CancellationToken.None);

                logger?.LogInformation("Unleash Feature Flags loaded successfully.");
            }
            catch (OperationCanceledException)
            {
                logger?.LogWarning("Unleash Feature Flags load timed out.");
            }

            return next;
        }
    }
}
