using System;
using System.Collections.Generic;

namespace Unleash.Metrics
{
    internal class ClientRegistration : BaseMetrics
    {
        public string SdkVersion { get; set; }
        public List<string> Strategies { get; set; }
        public DateTimeOffset Started { get; set; }
        public long Interval { get; set; }
    }
}