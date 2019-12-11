using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unleash
{
    public static class ServiceCollectionExtensions
    {
        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
#if NETCOREAPP3_0
            Microsoft.Extensions.Hosting.IHostEnvironment hostEnvironment
#else
            Microsoft.Extensions.Hosting.IHostingEnvironment hostEnvironment
#endif
            )
        {
            return serviceCollection.AddUnleash(
                settingsInitializer: null,
                configuration: null,
                settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostEnvironment.EnvironmentName;
                    }
                });
        }

        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
#if NETCOREAPP3_0
            Microsoft.Extensions.Hosting.IHostEnvironment hostEnvironment,
#else
            Microsoft.Extensions.Hosting.IHostingEnvironment hostEnvironment,
#endif
            IConfiguration configuration)
        {
            return serviceCollection.AddUnleash(
                settingsInitializer: null,
                configuration: configuration,
                settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostEnvironment.EnvironmentName;
                    }
                });
        }

        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
#if NETCOREAPP3_0
            Microsoft.Extensions.Hosting.IHostEnvironment hostEnvironment,
#else
            Microsoft.Extensions.Hosting.IHostingEnvironment hostEnvironment,
#endif
            Action<UnleashSettings> settingsInitializer)
        {
            return serviceCollection.AddUnleash(
                settingsInitializer,
                configuration: null,
                settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostEnvironment.EnvironmentName;
                    }
                });
        }

        public static IUnleashServiceCollection AddUnleash(
            this IServiceCollection serviceCollection,
#if NETCOREAPP3_0
            Microsoft.Extensions.Hosting.IHostEnvironment hostEnvironment,
#else
            Microsoft.Extensions.Hosting.IHostingEnvironment hostEnvironment,
#endif
            IConfiguration configuration,
            Action<UnleashSettings> settingInitializer)
        {
            return serviceCollection.AddUnleash(
                settingInitializer,
                configuration,
                settings =>
                {
                    if (string.IsNullOrEmpty(settings.InstanceTag))
                    {
                        settings.InstanceTag = hostEnvironment.EnvironmentName;
                    }
                });
        }
    }
}
