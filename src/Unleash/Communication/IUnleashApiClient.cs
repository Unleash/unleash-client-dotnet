using System.Threading;
using System.Threading.Tasks;
using Unleash.Metrics;
using EngineBucket = Yggdrasil.MetricsBucket;

namespace Unleash.Communication
{
    internal interface IUnleashApiClient
    {
        Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken);
        Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken);
        Task<bool> SendMetrics(ThreadSafeMetricsBucket metrics, CancellationToken cancellationToken);
        Task<bool> SendEngineMetrics(EngineBucket metrics, CancellationToken cancellationToken);
    }
}