using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Internal;
using Unleash.Lifetime;

namespace Unleash
{
    public static class UnleashServiceCollectionExtensions
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        public static IUnleashServiceCollection WithSynchronousFlagLoadingOnStartup(this IUnleashServiceCollection serviceCollection, bool enabled = true, bool onlyOnEmptyCache = false, TimeSpan? timeout = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (enabled)
            {
                serviceCollection.AddTransient<IStartupFilter>(
                    sp =>
                    {
                        var services = sp.GetRequiredService<IUnleashServices>();
                        return new SynchronousFlagLoadingStartupFilter(
                            sp, services, onlyOnEmptyCache, timeout ?? DefaultTimeout);
                    });
            }

            return serviceCollection;
        }

        public static IUnleashServiceCollection WithWebHostControlledLifetime(this IUnleashServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IStartupFilter, WebHostControlledLifetimeStartupFilter>();
            return serviceCollection;
        }
    }
}
