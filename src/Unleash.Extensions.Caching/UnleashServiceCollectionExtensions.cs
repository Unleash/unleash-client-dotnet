using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Utility;

namespace Unleash
{
    using Caching;

    public static class UnleashServiceCollectionExtensions
    {
        public static IUnleashServiceCollection WithDistributedToggleCollectionCache(
            this IUnleashServiceCollection serviceCollection,
            Action<DistributedToggleCollectionCacheSettings> settingsConfigurator = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var settings = new DistributedToggleCollectionCacheSettings();

            if (serviceCollection.UnleashConfiguration != null)
            {
                var section = serviceCollection.UnleashConfiguration.GetSection("Caching:Distributed");
                section.Bind(settings);
            }

            settingsConfigurator?.Invoke(settings);

            SettingsValidator.Validate(settings);

            serviceCollection.AddSingleton(settings);

            serviceCollection.WithToggleCollectionCache<DistributedToggleCollectionCache>();

            return serviceCollection;
        }

        public static IUnleashServiceCollection WithMemoryToggleCollectionCache(this IUnleashServiceCollection serviceCollection, Action<MemoryToggleCollectionCacheSettings> settingsConfigurator = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var settings = new MemoryToggleCollectionCacheSettings();

            if (serviceCollection.UnleashConfiguration != null)
            {
                var section = serviceCollection.UnleashConfiguration.GetSection("Caching:Memory");
                section.Bind(settings);
            }

            settingsConfigurator?.Invoke(settings);

            SettingsValidator.Validate(settings);

            serviceCollection.AddSingleton(settings);

            serviceCollection.AddMemoryCache();
            serviceCollection.WithToggleCollectionCache<MemoryToggleCollectionCache>();

            return serviceCollection;
        }
    }
}
