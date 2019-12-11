namespace Unleash.Communication.Admin.Dto
{
    public class EventsResult
    {
        public int Version { get; set; }
        public EventEntry[] Events { get; set; }
    }
}