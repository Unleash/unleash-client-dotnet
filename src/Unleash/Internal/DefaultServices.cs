using Unleash.Caching;
using Unleash.Communication;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Strategies;

namespace Unleash.Internal
{
    internal class DefaultServices : UnleashServices
    {
        private static readonly IJsonSerializer DefaultJsonSerializer = new DynamicNewtonsoftJsonSerializer();
        private static readonly IHttpClientFactory DefaultHttpClientFactory = new DefaultHttpClientFactory();
        private static readonly IUnleashScheduledTaskManager ScheduledTaskManager = new SystemTimerScheduledTaskManager();

        private static readonly IStrategy[] DefaultStrategies = {
            new DefaultStrategy(),
            new UserWithIdStrategy(),
            new GradualRolloutUserIdStrategy(),
            new GradualRolloutRandomStrategy(),
            new ApplicationHostnameStrategy(),
            new GradualRolloutSessionIdStrategy(),
            new RemoteAddressStrategy()
        };

        /// <inheritdoc />
        public DefaultServices(UnleashSettings settings)
            : base(
                settings,
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
                DefaultStrategies)
        {
        }
    }
}
