using System.Collections.Generic;

namespace Unleash.Communication.Admin.Dto
{
    public class EventData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Strategy { get; set; }
        public bool Enabled { get; set; }
        public object Parameters { get; set; }
    }
}
