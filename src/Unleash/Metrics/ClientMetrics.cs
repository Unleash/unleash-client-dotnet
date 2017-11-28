namespace Unleash.Metrics
{
    internal class ClientMetrics
    {
        public string AppName { get; set; }
        public string InstanceId { get; set; }
        public MetricsBucket Bucket { get; set; }
    }
}