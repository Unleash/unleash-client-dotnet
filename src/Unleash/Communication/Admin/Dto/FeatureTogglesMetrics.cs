using System.Collections.Generic;

namespace Unleash.Communication.Admin.Dto
{
    public class FeatureTogglesMetrics
    {
        public Dictionary<string, FeatureTogglesMetricsEntry> LastHour { get; set; }
        public Dictionary<string, FeatureTogglesMetricsEntry> LastMinute { get; set; }
    }
}