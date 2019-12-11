using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.GenericHost
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTimedService<TService, TOptions, TState>(
            this IServiceCollection services,
            Action<TOptions> optionsConfigurator)
                where TService : class, ITimerInvokedService<TState>
                where TOptions : TimerInvokedServiceOptions
                where TState : class
        {
            services.Configure(optionsConfigurator);
            services.AddSingleton<TState>();
            services.AddScoped<TService>();
            services.AddHostedService<TimerExecutorService<TService>>();

            return services;
        }

        public static IServiceCollection AddTimedService<TService, TOptions>(
            this IServiceCollection services,
            Action<TOptions> optionsConfigurator)
            where TService : class, ITimerInvokedService
            where TOptions : TimerInvokedServiceOptions
        {
            services.Configure(optionsConfigurator);
            services.AddScoped<TService>();
            services.AddHostedService<TimerExecutorService<TService>>();

            return services;
        }

        public static IServiceCollection AddTimedService<TService, TState>(
            this IServiceCollection services,
            Action<TimerInvokedServiceOptions> optionsConfigurator)
            where TService : class, ITimerInvokedService<TState>
            where TState : class
        {
            services.Configure(optionsConfigurator);
            services.AddSingleton<TState>();
            services.AddScoped<TService>();
            services.AddHostedService<TimerExecutorService<TService>>();

            return services;
        }

        public static IServiceCollection AddTimedService<TService>(
            this IServiceCollection services,
            Action<TimerInvokedServiceOptions> optionsConfigurator)
            where TService : class, ITimerInvokedService
        {
            services.Configure(optionsConfigurator);
            services.AddScoped<TService>();
            services.AddHostedService<TimerExecutorService<TService>>();

            return services;
        }
    }
}
