using System.Collections.Generic;

namespace Unleash.Communication
{
    internal class UnleashApiClientRequestHeaders
    {
        public string AppName { get; set; }   
        public string InstanceId { get; set; }   
        public Dictionary<string,string> CustomHttpHeaders { get; set; }   
    }
}