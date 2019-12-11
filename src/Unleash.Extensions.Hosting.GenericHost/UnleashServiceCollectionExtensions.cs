using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Unleash.Lifetime;

namespace Unleash
{
    public static class UnleashServiceCollectionExtensions
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        public static IUnleashServiceCollection WithSynchronousFlagLoadingOnStartup(
            this IUnleashServiceCollection serviceCollection,
            bool onlyOnEmptyCache = false,
            TimeSpan? timeout = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.Configure<SynchronousFlagLoadingServiceOptions>(opt =>
            {
                opt.OnlyOnEmptyCache = onlyOnEmptyCache;
                opt.Timeout = timeout ?? DefaultTimeout;
            });

            serviceCollection.AddHostedService<SynchronousFlagLoadingService>();

            return serviceCollection;
        }

        public static IUnleashServiceCollection WithHostControlledLifetime(this IUnleashServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IHostedService, HostControlledLifetimeService>();

            return serviceCollection;
        }
    }
}
