namespace Sample.GenericHost.HostedServices
{
    public class UnleashSampleServiceOptions : TimerInvokedServiceOptions
    {
        public string SessionId { get; set; }
        public string FeatureToggleName { get; set; }
        public string ExtraParamName { get; set; }
        public string ExtraParamValue { get; set; }
    }
}
