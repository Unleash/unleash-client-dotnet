using System;
using System.Collections.Generic;

namespace Unleash.Metrics
{
    internal class ClientRegistration
    {
        //public ClientRegistration(UnleashConfig config, DateTimeOffset started, List<string> strategies)
        //{
        //    AppName = config.AppName;
        //    InstanceId = config.InstanceId;
        //    SdkVersion = config.SdkVersion;
        //    Started = started;
        //    Strategies = strategies;
        //    Interval = (long) config.SendMetricsInterval.TotalSeconds;
        //}

        public string AppName { get; set; }
        public string InstanceId { get; set; }
        public string SdkVersion { get; set; }
        public List<string> Strategies { get; set; }
        public DateTimeOffset Started { get; set; }
        public long Interval { get; set; }
    }
}