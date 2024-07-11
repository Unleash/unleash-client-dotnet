namespace Unleash.Metrics
{
    internal class ClientMetrics
    {
        public string AppName { get; set; }
        public string InstanceId { get; set; }
        public Yggdrasil.MetricsBucket Bucket { get; set; }
        public string PlatformName
        {
            get
            {
                return MetricsMetadata.GetPlatformName();

            }
        }
        public string PlatformVersion
        {
            get
            {
                return MetricsMetadata.GetPlatformVersion();
            }
        }
        public string YggdrasilVersion
        {
            get
            {
                return null;
            }
        }
        public string SpecVersion
        {
            get
            {
                return UnleashServices.supportedSpecVersion;
            }
        }
    }
}