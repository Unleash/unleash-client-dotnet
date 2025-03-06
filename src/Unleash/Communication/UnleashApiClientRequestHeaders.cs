using System;
using System.Collections.Generic;

namespace Unleash.Communication
{
    internal class UnleashApiClientRequestHeaders
    {
        public string AppName { get; set; }
        public string InstanceTag { get; set; }
        public string ConnectionId { get; internal set; }
        public string SdkVersion { get; set; }
        public Dictionary<string, string> CustomHttpHeaders { get; set; }
        public IUnleashCustomHttpHeaderProvider CustomHttpHeaderProvider { get; set; }
        public string SupportedSpecVersion { get; internal set; }
        public TimeSpan SendMetricsInterval { get; set; }
        public TimeSpan FetchTogglesInterval { get; set; }

    }
}