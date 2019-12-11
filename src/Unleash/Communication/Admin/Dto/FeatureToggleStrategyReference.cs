using System.Collections.Generic;

namespace Unleash.Communication.Admin.Dto
{
    public class FeatureToggleStrategyReference
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}