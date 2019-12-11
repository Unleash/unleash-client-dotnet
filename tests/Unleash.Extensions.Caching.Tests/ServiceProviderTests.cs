using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Caching;
using Unleash.Extensions.DependencyInjection.Tests.Helpers;
using Xunit;

namespace Unleash.Extensions.Caching.Tests
{
    public class ServiceProviderTests
    {
        private static readonly TimeSpan OneSecond = TimeSpan.Zero.Add(TimeSpan.FromSeconds(1));
        private static readonly DateTimeOffset SomeDate = new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);

        [Fact]
        public void WithMemoryToggleCollectionCache_WithNoConfiguration_RegistersNecessaryServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUnleash(s =>
                {
                    s.UnleashApi = new Uri("http://localhost:4242/");
                    s.AppName = "Test";
                    s.InstanceTag = "Test";
                })
                .WithMemoryToggleCollectionCache();

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<MemoryToggleCollectionCache>(toggleCollectionCache);
        }

        [Fact]
        public void WithMemoryToggleCollectionCache_WithConfiguration_ConfiguresAppropriately()
        {
            var serviceCollection = new ServiceCollection();
            var unleashConfiguration = CreateMemoryToggleCollectionCacheConfiguration();

            serviceCollection.AddUnleash(unleashConfiguration)
                .WithMemoryToggleCollectionCache();

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<MemoryToggleCollectionCache>(toggleCollectionCache);

            var toggleCollectionCacheWithSettings = (IToggleCollectionCache<MemoryToggleCollectionCacheSettings>) toggleCollectionCache;
            var settings = toggleCollectionCacheWithSettings.Settings;

            Assert.Equal("Toggles", settings.ToggleCollectionKeyName);
            Assert.Equal("Etag", settings.EtagKeyName);

            Assert.Equal(CacheItemPriority.High, settings.EtagEntryOptions.Priority);
            Assert.Equal(long.MaxValue, settings.EtagEntryOptions.Size);
            Assert.Equal(DateTimeOffset.MaxValue, settings.EtagEntryOptions.AbsoluteExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.SlidingExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.AbsoluteExpirationRelativeToNow);

            Assert.Equal(CacheItemPriority.Low, settings.ToggleCollectionEntryOptions.Priority);
            Assert.Equal(1024, settings.ToggleCollectionEntryOptions.Size);
            Assert.Equal(SomeDate, settings.ToggleCollectionEntryOptions.AbsoluteExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.SlidingExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void WithMemoryToggleCollectionCache_WithLambdaBasedConfiguration_ConfiguresAppropriately()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUnleash(s =>
                {
                    s.UnleashApi = new Uri("http://localhost:4242/");
                    s.AppName = "Test";
                    s.InstanceTag = "Test";
                })
                .WithMemoryToggleCollectionCache(m =>
                {
                    m.ToggleCollectionKeyName = "Toggles";
                    m.EtagKeyName = "Etag";
                    m.EtagEntryOptions = new MemoryCacheEntryOptions
                    {
                        Priority = CacheItemPriority.High,
                        Size = long.MaxValue,
                        AbsoluteExpiration = DateTimeOffset.MaxValue,
                        SlidingExpiration = TimeSpan.MaxValue,
                        AbsoluteExpirationRelativeToNow = TimeSpan.MaxValue
                    };
                    m.ToggleCollectionEntryOptions = new MemoryCacheEntryOptions
                    {
                        Priority = CacheItemPriority.Low,
                        Size = 1024,
                        AbsoluteExpiration = SomeDate,
                        SlidingExpiration = OneSecond,
                        AbsoluteExpirationRelativeToNow = OneSecond
                    };
                });

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<MemoryToggleCollectionCache>(toggleCollectionCache);

            var toggleCollectionCacheWithSettings = (IToggleCollectionCache<MemoryToggleCollectionCacheSettings>) toggleCollectionCache;
            var settings = toggleCollectionCacheWithSettings.Settings;

            Assert.Equal("Toggles", settings.ToggleCollectionKeyName);
            Assert.Equal("Etag", settings.EtagKeyName);

            Assert.Equal(CacheItemPriority.High, settings.EtagEntryOptions.Priority);
            Assert.Equal(long.MaxValue, settings.EtagEntryOptions.Size);
            Assert.Equal(DateTimeOffset.MaxValue, settings.EtagEntryOptions.AbsoluteExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.SlidingExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.AbsoluteExpirationRelativeToNow);

            Assert.Equal(CacheItemPriority.Low, settings.ToggleCollectionEntryOptions.Priority);
            Assert.Equal(1024, settings.ToggleCollectionEntryOptions.Size);
            Assert.Equal(SomeDate, settings.ToggleCollectionEntryOptions.AbsoluteExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.SlidingExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void WithMemoryToggleCollectionCache_WithConfigurationAndLambdaOverrides_ConfiguresAppropriately()
        {
            var serviceCollection = new ServiceCollection();
            var unleashConfiguration = CreateMemoryToggleCollectionCacheConfiguration();

            serviceCollection.AddUnleash(unleashConfiguration)
                .WithMemoryToggleCollectionCache(m =>
                {
                    m.ToggleCollectionKeyName = m.ToggleCollectionKeyName + m.ToggleCollectionKeyName;
                    m.EtagKeyName = m.EtagKeyName + m.EtagKeyName;

                    m.EtagEntryOptions.Priority = (CacheItemPriority) ((int) m.EtagEntryOptions.Priority - 1);
                    m.EtagEntryOptions.Size = m.EtagEntryOptions.Size - 1;
                    m.EtagEntryOptions.AbsoluteExpiration = m.EtagEntryOptions.AbsoluteExpiration?.AddDays(-1);
                    m.EtagEntryOptions.SlidingExpiration = m.EtagEntryOptions.SlidingExpiration?.Add(-1 * TimeSpan.FromHours(1));
                    m.EtagEntryOptions.AbsoluteExpirationRelativeToNow = m.EtagEntryOptions.AbsoluteExpirationRelativeToNow?.Add(-1 * TimeSpan.FromHours(1));

                    m.ToggleCollectionEntryOptions.Priority = (CacheItemPriority) ((int) m.ToggleCollectionEntryOptions.Priority + 1);
                    m.ToggleCollectionEntryOptions.Size = m.ToggleCollectionEntryOptions.Size + 1;
                    m.ToggleCollectionEntryOptions.AbsoluteExpiration = m.ToggleCollectionEntryOptions.AbsoluteExpiration?.AddDays(1);
                    m.ToggleCollectionEntryOptions.SlidingExpiration = m.ToggleCollectionEntryOptions.SlidingExpiration?.Add(TimeSpan.FromHours(1));
                    m.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow = m.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow?.Add(TimeSpan.FromHours(1));
                });

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<MemoryToggleCollectionCache>(toggleCollectionCache);

            var toggleCollectionCacheWithSettings = (IToggleCollectionCache<MemoryToggleCollectionCacheSettings>) toggleCollectionCache;
            var settings = toggleCollectionCacheWithSettings.Settings;

            Assert.Equal("TogglesToggles", settings.ToggleCollectionKeyName);
            Assert.Equal("EtagEtag", settings.EtagKeyName);

            Assert.Equal(CacheItemPriority.Normal, settings.EtagEntryOptions.Priority);
            Assert.Equal(long.MaxValue - 1, settings.EtagEntryOptions.Size);
            Assert.Equal(DateTimeOffset.MaxValue.AddDays(-1), settings.EtagEntryOptions.AbsoluteExpiration);
            Assert.Equal(TimeSpan.MaxValue.Add(-1 * TimeSpan.FromHours(1)), settings.EtagEntryOptions.SlidingExpiration);
            Assert.Equal(TimeSpan.MaxValue.Add(-1 * TimeSpan.FromHours(1)), settings.EtagEntryOptions.AbsoluteExpirationRelativeToNow);

            Assert.Equal(CacheItemPriority.Normal, settings.ToggleCollectionEntryOptions.Priority);
            Assert.Equal(1025, settings.ToggleCollectionEntryOptions.Size);
            Assert.Equal(SomeDate.AddDays(1), settings.ToggleCollectionEntryOptions.AbsoluteExpiration);
            Assert.Equal(OneSecond.Add(TimeSpan.FromHours(1)), settings.ToggleCollectionEntryOptions.SlidingExpiration);
            Assert.Equal(OneSecond.Add(TimeSpan.FromHours(1)), settings.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void WithDistributedToggleCollectionCache_WithNoConfiguration_RegistersNecessaryServices()
        {
            var serviceCollection = new ServiceCollection();

            // Use the in-memory distributed cache for testing purposes.
            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.AddUnleash(s =>
                {
                    s.UnleashApi = new Uri("http://localhost:4242/");
                    s.AppName = "Test";
                    s.InstanceTag = "Test";
                })
                .WithNewtonsoftJsonSerializer()
                .WithDistributedToggleCollectionCache();

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<DistributedToggleCollectionCache>(toggleCollectionCache);
        }


        [Fact]
        public void WithDistributedToggleCollectionCache_WithConfiguration_ConfiguresAppropriately()
        {
            var serviceCollection = new ServiceCollection();
            var unleashConfiguration = CreateDistributedToggleCollectionCacheConfiguration();

            // Use the in-memory distributed cache for testing purposes.
            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.AddUnleash(unleashConfiguration)
                .WithNewtonsoftJsonSerializer()
                .WithDistributedToggleCollectionCache();

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<DistributedToggleCollectionCache>(toggleCollectionCache);

            var toggleCollectionCacheWithSettings = (IToggleCollectionCache<DistributedToggleCollectionCacheSettings>) toggleCollectionCache;
            var settings = toggleCollectionCacheWithSettings.Settings;

            Assert.Equal("Toggles", settings.ToggleCollectionKeyName);
            Assert.Equal("Etag", settings.EtagKeyName);

            Assert.Equal(DateTimeOffset.MaxValue, settings.EtagEntryOptions.AbsoluteExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.SlidingExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.AbsoluteExpirationRelativeToNow);

            Assert.Equal(SomeDate, settings.ToggleCollectionEntryOptions.AbsoluteExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.SlidingExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void WithDistributedToggleCollectionCache_WithLambdaBasedConfiguration_ConfiguresAppropriately()
        {
            var serviceCollection = new ServiceCollection();

            // Use the in-memory distributed cache for testing purposes.
            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.AddUnleash(s =>
                {
                    s.UnleashApi = new Uri("http://localhost:4242/");
                    s.AppName = "Test";
                    s.InstanceTag = "Test";
                })
                .WithNewtonsoftJsonSerializer()
                .WithDistributedToggleCollectionCache(d =>
                {
                    d.ToggleCollectionKeyName = "Toggles";
                    d.EtagKeyName = "Etag";
                    d.EtagEntryOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.MaxValue,
                        SlidingExpiration = TimeSpan.MaxValue,
                        AbsoluteExpirationRelativeToNow = TimeSpan.MaxValue
                    };
                    d.ToggleCollectionEntryOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = SomeDate,
                        SlidingExpiration = OneSecond,
                        AbsoluteExpirationRelativeToNow = OneSecond
                    };
                });

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<DistributedToggleCollectionCache>(toggleCollectionCache);

            var toggleCollectionCacheWithSettings = (IToggleCollectionCache<DistributedToggleCollectionCacheSettings>) toggleCollectionCache;
            var settings = toggleCollectionCacheWithSettings.Settings;

            Assert.Equal("Toggles", settings.ToggleCollectionKeyName);
            Assert.Equal("Etag", settings.EtagKeyName);

            Assert.Equal(DateTimeOffset.MaxValue, settings.EtagEntryOptions.AbsoluteExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.SlidingExpiration);
            Assert.Equal(TimeSpan.MaxValue, settings.EtagEntryOptions.AbsoluteExpirationRelativeToNow);

            Assert.Equal(SomeDate, settings.ToggleCollectionEntryOptions.AbsoluteExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.SlidingExpiration);
            Assert.Equal(OneSecond, settings.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void WithDistributedToggleCollectionCache_WithConfigurationAndLambdaOverrides_ConfiguresAppropriately()
        {
            var serviceCollection = new ServiceCollection();
            var unleashConfiguration = CreateDistributedToggleCollectionCacheConfiguration();

            // Use the in-memory distributed cache for testing purposes.
            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.AddUnleash(unleashConfiguration)
                .WithNewtonsoftJsonSerializer()
                .WithDistributedToggleCollectionCache(d =>
                {
                    d.ToggleCollectionKeyName = d.ToggleCollectionKeyName + d.ToggleCollectionKeyName;
                    d.EtagKeyName = d.EtagKeyName + d.EtagKeyName;

                    d.EtagEntryOptions.AbsoluteExpiration = d.EtagEntryOptions.AbsoluteExpiration?.AddDays(-1);
                    d.EtagEntryOptions.SlidingExpiration = d.EtagEntryOptions.SlidingExpiration?.Add(-1 * TimeSpan.FromHours(1));
                    d.EtagEntryOptions.AbsoluteExpirationRelativeToNow = d.EtagEntryOptions.AbsoluteExpirationRelativeToNow?.Add(-1 * TimeSpan.FromHours(1));

                    d.ToggleCollectionEntryOptions.AbsoluteExpiration = d.ToggleCollectionEntryOptions.AbsoluteExpiration?.AddDays(1);
                    d.ToggleCollectionEntryOptions.SlidingExpiration = d.ToggleCollectionEntryOptions.SlidingExpiration?.Add(TimeSpan.FromHours(1));
                    d.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow = d.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow?.Add(TimeSpan.FromHours(1));
                });

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var toggleCollectionCache = serviceProvider.GetRequiredService<IToggleCollectionCache>();
            Assert.IsType<DistributedToggleCollectionCache>(toggleCollectionCache);

            var toggleCollectionCacheWithSettings = (IToggleCollectionCache<DistributedToggleCollectionCacheSettings>) toggleCollectionCache;
            var settings = toggleCollectionCacheWithSettings.Settings;

            Assert.Equal("TogglesToggles", settings.ToggleCollectionKeyName);
            Assert.Equal("EtagEtag", settings.EtagKeyName);

            Assert.Equal(DateTimeOffset.MaxValue.AddDays(-1), settings.EtagEntryOptions.AbsoluteExpiration);
            Assert.Equal(TimeSpan.MaxValue.Add(-1 * TimeSpan.FromHours(1)), settings.EtagEntryOptions.SlidingExpiration);
            Assert.Equal(TimeSpan.MaxValue.Add(-1 * TimeSpan.FromHours(1)), settings.EtagEntryOptions.AbsoluteExpirationRelativeToNow);

            Assert.Equal(SomeDate.AddDays(1), settings.ToggleCollectionEntryOptions.AbsoluteExpiration);
            Assert.Equal(OneSecond.Add(TimeSpan.FromHours(1)), settings.ToggleCollectionEntryOptions.SlidingExpiration);
            Assert.Equal(OneSecond.Add(TimeSpan.FromHours(1)), settings.ToggleCollectionEntryOptions.AbsoluteExpirationRelativeToNow);
        }

        private static IConfiguration CreateMemoryToggleCollectionCacheConfiguration()
        {
            return UnleashConfigurationBuilder.Create()
                .AddSection(
                    "Caching:Memory",
                    ("ToggleCollectionKeyName", "Toggles"),
                    ("ToggleCollectionKeyName", "Toggles"),
                    ("EtagKeyName", "Etag"),
                    ("EtagEntryOptions:Priority", "High"),
                    ("EtagEntryOptions:Size", "9223372036854775807"),
                    ("EtagEntryOptions:AbsoluteExpiration", "12/31/9999 11:59:59.9999999 PM +00:00"),
                    ("EtagEntryOptions:SlidingExpiration", "10675199.02:48:05.4775807"),
                    ("EtagEntryOptions:AbsoluteExpirationRelativeToNow", "10675199.02:48:05.4775807"),
                    ("ToggleCollectionEntryOptions:Priority", "Low"),
                    ("ToggleCollectionEntryOptions:Size", "1024"),
                    ("ToggleCollectionEntryOptions:AbsoluteExpiration", "01/01/2100 00:00:00.0000000 AM +00:00"),
                    ("ToggleCollectionEntryOptions:SlidingExpiration", "00:00:01"),
                    ("ToggleCollectionEntryOptions:AbsoluteExpirationRelativeToNow", "00:00:01")
                )
                .Build();
        }

        private static IConfiguration CreateDistributedToggleCollectionCacheConfiguration()
        {
            return UnleashConfigurationBuilder.Create()
                .AddSection(
                    "Caching:Distributed",
                    ("ToggleCollectionKeyName", "Toggles"),
                    ("ToggleCollectionKeyName", "Toggles"),
                    ("EtagKeyName", "Etag"),
                    ("EtagEntryOptions:AbsoluteExpiration", "12/31/9999 11:59:59.9999999 PM +00:00"),
                    ("EtagEntryOptions:SlidingExpiration", "10675199.02:48:05.4775807"),
                    ("EtagEntryOptions:AbsoluteExpirationRelativeToNow", "10675199.02:48:05.4775807"),
                    ("ToggleCollectionEntryOptions:AbsoluteExpiration", "01/01/2100 00:00:00.0000000 AM +00:00"),
                    ("ToggleCollectionEntryOptions:SlidingExpiration", "00:00:01"),
                    ("ToggleCollectionEntryOptions:AbsoluteExpirationRelativeToNow", "00:00:01")
                )
                .Build();
        }
    }
}
