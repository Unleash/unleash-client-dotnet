using System;
using System.Collections.Generic;
using Unleash.Util;

namespace Unleash.Metrics
{
    internal class ClientRegistration
    {
        public ClientRegistration(UnleashConfig config, DateTimeOffset started, List<string> strategies)
        {
            AppName = config.AppName;
            InstanceId = config.InstanceId;
            SdkVersion = config.SdkVersion;
            Started = started;
            Strategies = strategies;
            Interval = (long) config.SendMetricsInterval.TotalSeconds;
        }

        public string AppName { get; }
        public string InstanceId { get; }
        public string SdkVersion { get; }
        public List<string> Strategies { get; }
        public DateTimeOffset Started { get; }
        public long Interval { get; }
    }
}