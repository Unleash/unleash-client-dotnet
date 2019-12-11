using System.Threading;
using System.Threading.Tasks;

namespace Unleash.Communication.Admin
{
    public interface IUnleashAdminApiClient
    {
        IUnleashAdminFeatureTogglesApiClient FeatureToggles { get; }
        IUnleashAdminStrategiesApiClient Strategies { get; }
        IUnleashAdminMetricsApiClient Metrics { get; }
        IUnleashAdminEventsApiClient Events { get; }
        IUnleashAdminStateApiClient State { get; }

        Task Authenticate(string emailAddress, CancellationToken cancellationToken = default(CancellationToken));
    }
}
