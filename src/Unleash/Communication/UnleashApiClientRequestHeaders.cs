using System.Collections.Generic;

namespace Unleash.Communication
{
    internal class UnleashApiClientRequestHeaders
    {
        public string AppName { get; set; }   
        public string InstanceTag { get; set; }   
        public Dictionary<string,string> CustomHttpHeaders { get; set; }
        public IUnleashCustomHttpHeaderProvider CustomHttpHeaderProvider { get; set; }
        public string SupportedSpecVersion { get; internal set; }
    }
}