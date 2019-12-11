using System;

namespace Unleash.Communication.Admin.Dto
{
    public class EventEntry
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public EventData Data { get; set; }
        public EventDiff[] Diffs { get; set; }
    }
}