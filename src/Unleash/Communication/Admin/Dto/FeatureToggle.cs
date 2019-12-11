using System.Collections.Generic;

namespace Unleash.Communication.Admin.Dto
{
    public class FeatureToggle
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public FeatureToggleStrategyReference[] Strategies { get; set; }
        public Variant[] Variants { get; set; }
        public string Strategy { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}