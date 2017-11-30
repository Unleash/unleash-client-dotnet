using System;
using System.Collections.Generic;

namespace Unleash.Metrics
{
    internal class ClientRegistration
    {
        public string AppName { get; set; }
        public string InstanceId { get; set; }
        public string SdkVersion { get; set; }
        public List<string> Strategies { get; set; }
        public DateTimeOffset Started { get; set; }
        public long Interval { get; set; }
    }
}