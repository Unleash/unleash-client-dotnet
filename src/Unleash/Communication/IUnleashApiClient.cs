using System.Threading;
using System.Threading.Tasks;
using Yggdrasil;

namespace Unleash.Communication
{
    internal interface IUnleashApiClient
    {
        Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken, bool throwOnFail = false);
        Task<bool> RegisterClient(Metrics.ClientRegistration registration, CancellationToken cancellationToken);
        Task<bool> SendMetrics(MetricsBucket metrics, CancellationToken cancellationToken);
    }
}