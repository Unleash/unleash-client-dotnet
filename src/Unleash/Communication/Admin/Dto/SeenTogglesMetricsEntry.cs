namespace Unleash.Communication.Admin.Dto
{
    public class SeenTogglesMetricsEntry
    {
        public string AppName { get; set; }
        public string[] SeenToggles { get; set; }
        public int MetricsCount { get; set; }
    }
}