using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unleash.Internal;

namespace Unleash.Lifetime
{
    public class WebHostControlledLifetimeStartupFilter : IStartupFilter
    {
        private readonly IUnleashServices unleashServices;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IApplicationLifetime applicationLifetime;

        public WebHostControlledLifetimeStartupFilter(
            IUnleashServices unleashServices,
            IHostingEnvironment hostingEnvironment,
            IApplicationLifetime applicationLifetime)
        {
            this.unleashServices = unleashServices;
            this.hostingEnvironment = hostingEnvironment;
            this.applicationLifetime = applicationLifetime;
        }

        /// <inheritdoc />
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var logger = app.ApplicationServices.GetService<ILogger<WebHostControlledLifetimeStartupFilter>>();

                applicationLifetime.ApplicationStopping.Register(() =>
                {
                    logger?.LogInformation("Shutting down Unleash");
                    unleashServices.Dispose();
                });

                next(app);
            };
        }
    }
}
