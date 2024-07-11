using Yggdrasil;

namespace Unleash.Metrics
{
    internal class ClientMetrics: BaseMetrics
    {
        public MetricsBucket Bucket { get; set; }
    }
}