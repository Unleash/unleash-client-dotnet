using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Caching;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Strategies;

namespace Unleash
{
    public static class ServiceCollectionExtensions
    {
        public static IUnleashServiceCollection AddUnleash(this IServiceCollection serviceCollection, Action<UnleashSettings> settingsConfigurator = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var settings = new UnleashSettings();
            settingsConfigurator?.Invoke(settings);

            // UnleashSettings settings
            serviceCollection.AddSingleton(settings);
            serviceCollection.AddSingleton(serviceProvider => serviceProvider.GetService<IEnumerable<IStrategy>>()?.ToArray() ?? Array.Empty<IStrategy>());

            // Internal services
            serviceCollection.AddSingleton<IJsonSerializer, DynamicNewtonsoftJsonSerializer>();
            serviceCollection.AddSingleton(new UnleashApiClientRequestHeaders
            {
                AppName = settings.AppName,
                CustomHttpHeaders = settings.CustomHttpHeaders,
                InstanceTag = settings.InstanceTag
            });

            serviceCollection.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
            serviceCollection.AddSingleton<IUnleashApiClientFactory, DefaultUnleashApiClientFactory>();

            // serviceCollection.AddSingleton<IUnleashApiClientFactory, HttpClientFactoryApiClientFactory>();

            serviceCollection.AddSingleton<IUnleashApiClient, UnleashApiClient>();

            serviceCollection.AddSingleton<IUnleashServices, UnleashServices>();

            // Default: SystemTimer scheduled task manager
            serviceCollection.AddSingleton<IUnleashScheduledTaskManager, SystemTimerScheduledTaskManager>();

            // Default: Disk-based JSON toggle collection cache
            serviceCollection.AddSingleton<IFileSystem, FileSystem>();
            serviceCollection.AddSingleton<IToggleCollectionCache, FileSystemToggleCollectionCache>();

            serviceCollection.AddScoped<IUnleashContextProvider, DefaultUnleashContextProvider>();
            serviceCollection.AddScoped<IUnleash, DefaultUnleash>();

            return new UnleashServiceCollection(serviceCollection);
        }
    }
}
