using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Caching;
using Unleash.Communication;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Strategies;

namespace Unleash.Internal
{
    public class DefaultUnleashServices : IUnleashServices
    {
        private readonly UnleashServices _services;

        private static readonly IRandom DefaultRandom = new UnleashRandom();
        private static readonly IJsonSerializer DefaultJsonSerializer = new NewtonsoftJsonSerializer(new NewtonsoftJsonSerializerSettings());
        private static readonly IHttpClientFactory DefaultHttpClientFactory = new DefaultHttpClientFactory();
        private static readonly IUnleashScheduledTaskManager ScheduledTaskManager = new SystemTimerScheduledTaskManager();

        private static readonly IStrategy[] DefaultStrategies = {
            new DefaultStrategy(),
            new UserWithIdStrategy(),
            new GradualRolloutUserIdStrategy(),
            new GradualRolloutRandomStrategy(DefaultRandom),
            new ApplicationHostnameStrategy(),
            new GradualRolloutSessionIdStrategy(),
            new RemoteAddressStrategy()
        };

        public DefaultUnleashServices(UnleashSettings settings)
        {
            _services = new UnleashServices(
                settings,
                DefaultRandom,
                new DefaultUnleashApiClientFactory(
                    settings,
                    DefaultHttpClientFactory,
                    DefaultJsonSerializer,
                    new UnleashApiClientRequestHeaders
                    {
                        AppName = settings.AppName,
                        CustomHttpHeaders = settings.CustomHttpHeaders,
                        InstanceTag = settings.InstanceTag
                    }),
                ScheduledTaskManager,
                new FileSystemToggleCollectionCache(
                    settings,
                    DefaultJsonSerializer,
                    new FileSystem()),
                DefaultStrategies);
        }

        public IRandom Random => _services.Random;
        public IReadOnlyDictionary<string, IStrategy> StrategyMap => _services.StrategyMap;

        public Task FeatureToggleLoadComplete(
            bool onlyOnEmptyCache = true,
            CancellationToken cancellationToken = default(CancellationToken))
                => _services.FeatureToggleLoadComplete(onlyOnEmptyCache, cancellationToken);

        public ToggleCollection GetToggleCollection() => _services.GetToggleCollection();
        public void RegisterCount(string toggleName, bool enabled) => _services.RegisterCount(toggleName, enabled);
        public void Dispose() => _services.Dispose();
    }
}
