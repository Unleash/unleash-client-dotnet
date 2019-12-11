using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unleash
{
    public static class ServiceCollectionExtensions
    {
        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
            IHostingEnvironment hostingEnvironment)
        {
            return serviceCollection.AddUnleash(
                settingsInitializer: null,
                configuration: null,
                settingsOverrider: settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostingEnvironment.EnvironmentName;
                    }
                });
        }

        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
            IHostingEnvironment hostingEnvironment,
            IConfiguration configuration)
        {
            return serviceCollection.AddUnleash(
                settingsInitializer: null,
                configuration: configuration,
                settingsOverrider: settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostingEnvironment.EnvironmentName;
                    }
                });
        }

        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
            IHostingEnvironment hostingEnvironment,
            Action<UnleashSettings> settingsInitializer)
        {
            return serviceCollection.AddUnleash(
                settingsInitializer,
                configuration: null,
                settingsOverrider: settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostingEnvironment.EnvironmentName;
                    }
                });
        }

        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
            IHostingEnvironment hostingEnvironment,
            IConfiguration configuration,
            Action<UnleashSettings> settingInitializer)
        {
            return serviceCollection.AddUnleash(
                settingInitializer,
                configuration,
                settingsOverrider: settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostingEnvironment.EnvironmentName;
                    }
                });
        }
    }
}
