using System.Collections.Generic;
using Unleash.Variants;

namespace Unleash.Internal
{
    public class FeatureToggle
    {
        public FeatureToggle(string name, string type, bool enabled, bool impressionData, List<ActivationStrategy> strategies, List<VariantDefinition> variants = null)
        {
            Name = name;
            Type = type;
            Enabled = enabled;
            ImpressionData = impressionData;
            Strategies = strategies ?? new List<ActivationStrategy>();
            Variants = variants ?? new List<VariantDefinition>();
        }

        public string Name { get; }
        public string Type { get; }
        public bool Enabled { get; }
        public bool ImpressionData { get; }

        public List<ActivationStrategy> Strategies { get; }

        public List<VariantDefinition> Variants { get; }

        public override string ToString()
        {
            return $"FeatureToggle{{name=\'{Name}{'\''}, enabled={Enabled}, impressionData={ImpressionData}, strategies=\'{Strategies}{'\''}{'}'}";
        }
    }
}