using System;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Caching;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Strategies;

namespace Unleash
{
    public static class UnleashServiceCollectionExtensions
    {
        // Note: All strategies must be singletons b/c of the dependency on Random.  Random is seeded by DateTime which
        //       has a low resolution.  Rapidly creating new Randoms would have the effect of reducing randomness.
        public static IUnleashServiceCollection WithStrategy<TStrategy>(this IUnleashServiceCollection serviceCollection)
            where TStrategy : class, IStrategy
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IStrategy, TStrategy>();
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithStrategy<TStrategy>(this IUnleashServiceCollection serviceCollection, TStrategy strategy)
            where TStrategy : class, IStrategy
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IStrategy>(strategy);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithStrategy<TStrategy>(this IUnleashServiceCollection serviceCollection, Func<IServiceProvider, TStrategy> strategyFactory)
            where TStrategy : class, IStrategy
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IStrategy>(strategyFactory);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithDefaultStrategies(this IUnleashServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            return serviceCollection
                .WithStrategy<DefaultStrategy>()
                .WithStrategy<UserWithIdStrategy>()
                .WithStrategy<GradualRolloutUserIdStrategy>()
                .WithStrategy<GradualRolloutRandomStrategy>()
                .WithStrategy<ApplicationHostnameStrategy>()
                .WithStrategy<GradualRolloutSessionIdStrategy>()
                .WithStrategy<RemoteAddressStrategy>();
        }

        public static IUnleashServiceCollection WithScheduledTaskManager<TScheduledTaskManager>(this IUnleashServiceCollection serviceCollection)
            where TScheduledTaskManager : class, IUnleashScheduledTaskManager
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IUnleashScheduledTaskManager, TScheduledTaskManager>();
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithScheduledTaskManager<TScheduledTaskManager>(this IUnleashServiceCollection serviceCollection, TScheduledTaskManager scheduledTaskManager)
            where TScheduledTaskManager : class, IUnleashScheduledTaskManager
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IUnleashScheduledTaskManager>(scheduledTaskManager);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithScheduledTaskManager<TScheduledTaskManager>(this IUnleashServiceCollection serviceCollection, Func<IServiceProvider, TScheduledTaskManager> scheduledTaskManagerFactory)
            where TScheduledTaskManager : class, IUnleashScheduledTaskManager
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IUnleashScheduledTaskManager>(scheduledTaskManagerFactory);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithToggleCollectionCache<TToggleCollectionCache>(this IUnleashServiceCollection serviceCollection)
            where TToggleCollectionCache : class, IToggleCollectionCache
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IToggleCollectionCache, TToggleCollectionCache>();
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithToggleCollectionCache<TToggleCollectionCache>(this IUnleashServiceCollection serviceCollection, TToggleCollectionCache toggleCollectionCache)
            where TToggleCollectionCache : class, IToggleCollectionCache
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IToggleCollectionCache>(toggleCollectionCache);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithToggleCollectionCache<TToggleCollectionCache>(this IUnleashServiceCollection serviceCollection, Func<IServiceProvider, TToggleCollectionCache> toggleCollectionCacheFactory)
            where TToggleCollectionCache : class, IToggleCollectionCache
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IToggleCollectionCache, TToggleCollectionCache>(toggleCollectionCacheFactory);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithJsonSerializer<TJsonSerializer>(this IUnleashServiceCollection serviceCollection)
            where TJsonSerializer : class, IJsonSerializer
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IJsonSerializer, TJsonSerializer>();
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithJsonSerializer<TJsonSerializer>(this IUnleashServiceCollection serviceCollection, TJsonSerializer jsonSerializer)
            where TJsonSerializer : class, IJsonSerializer
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IJsonSerializer>(jsonSerializer);
            return serviceCollection;
        }

        public static IUnleashServiceCollection WithJsonSerializer<TJsonSerializer>(this IUnleashServiceCollection serviceCollection, Func<IServiceProvider, TJsonSerializer> jsonSerializerFactory)
            where TJsonSerializer : class, IJsonSerializer
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IJsonSerializer>(jsonSerializerFactory);
            return serviceCollection;
        }
    }
}
