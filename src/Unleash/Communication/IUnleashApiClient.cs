using System.Threading;
using System.Threading.Tasks;
using Unleash.Metrics;

namespace Unleash.Communication
{
    public interface IUnleashApiClient
    {
        Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken);
        Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken);
        Task<bool> SendMetrics(ThreadSafeMetricsBucket metrics, CancellationToken cancellationToken);
    }
}
