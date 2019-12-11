using System;

namespace Unleash.Communication.Admin.Dto
{
    public class Instance
    {
        public string InstanceId { get; set; }
        public string ClientIp { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}