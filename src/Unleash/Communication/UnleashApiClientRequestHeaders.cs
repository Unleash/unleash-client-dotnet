using System.Collections.Generic;

namespace Unleash.Communication
{
    public class UnleashApiClientRequestHeaders
    {
        public string AppName { get; set; }   
        public string InstanceTag { get; set; }   
        public Dictionary<string,string> CustomHttpHeaders { get; set; }   
    }
}
