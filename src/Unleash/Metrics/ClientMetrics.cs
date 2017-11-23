using Unleash.Util;

namespace Unleash.Metrics
{
    internal class ClientMetrics
    {
        internal ClientMetrics(UnleashConfig config, MetricsBucket bucket)
        {
            AppName = config.AppName;
            InstanceId = config.InstanceId;
            Bucket = bucket;
        }

        public string AppName { get; }
        public string InstanceId { get; }
        public MetricsBucket Bucket { get; }
    }
}