using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Caching;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Strategies;
using Unleash.Utility;

namespace Unleash
{
    public static class ServiceCollectionExtensions
    {
        public static IUnleashServiceCollection AddUnleash(this IServiceCollection serviceCollection) =>
            serviceCollection.AddUnleash(null, null, null);

        public static IUnleashServiceCollection AddUnleash(this IServiceCollection serviceCollection, IConfiguration configuration) =>
            serviceCollection.AddUnleash(null, configuration, null);

        public static IUnleashServiceCollection AddUnleash(this IServiceCollection serviceCollection, Action<UnleashSettings> settingsInitializer) =>
            serviceCollection.AddUnleash(settingsInitializer, null, null);

        public static IUnleashServiceCollection AddUnleash(this IServiceCollection serviceCollection,
            IConfiguration configuration, Action<UnleashSettings> settingInitializer) =>
            serviceCollection.AddUnleash(settingInitializer, configuration, null);

        internal static IUnleashServiceCollection AddUnleash(this IServiceCollection serviceCollection, Action<UnleashSettings> settingsInitializer, IConfiguration configuration, Action<UnleashSettings> settingsOverrider)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var result = new UnleashServiceCollection(serviceCollection, configuration);

            var settings = new UnleashSettings();
            settingsInitializer?.Invoke(settings);
            configuration?.Bind(settings);
            settingsOverrider?.Invoke(settings);
            SettingsValidator.Validate(settings);

            var unleashApiClientRequestHeaders = new UnleashApiClientRequestHeaders
            {
                AppName = settings.AppName,
                CustomHttpHeaders = settings.CustomHttpHeaders,
                InstanceTag = settings.InstanceTag
            };

            serviceCollection.AddSingleton(settings);
            serviceCollection.AddSingleton<IRandom>(new UnleashRandom());
            serviceCollection.AddSingleton(serviceProvider => serviceProvider.GetService<IEnumerable<IStrategy>>()?.ToArray() ?? Array.Empty<IStrategy>());

            // Internal services
            serviceCollection.AddSingleton<NewtonsoftJsonSerializerSettings>();
            serviceCollection.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
            serviceCollection.AddSingleton(unleashApiClientRequestHeaders);

            serviceCollection.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
            serviceCollection.AddSingleton<IUnleashApiClientFactory, DefaultUnleashApiClientFactory>();

            serviceCollection.AddSingleton<IUnleashApiClient, UnleashApiClient>();

            serviceCollection.AddSingleton<IUnleashServices, UnleashServices>();

            // Default: SystemTimer scheduled task manager
            serviceCollection.AddSingleton<IUnleashScheduledTaskManager, SystemTimerScheduledTaskManager>();

            // Default: Disk-based JSON toggle collection cache
            serviceCollection.AddSingleton<IFileSystem, FileSystem>();
            serviceCollection.AddSingleton<IToggleCollectionCache, FileSystemToggleCollectionCache>();

            serviceCollection.AddScoped<IUnleashContextProvider, DefaultUnleashContextProvider>();
            serviceCollection.AddScoped<IUnleash, Unleash>();

            return result;
        }
    }
}
