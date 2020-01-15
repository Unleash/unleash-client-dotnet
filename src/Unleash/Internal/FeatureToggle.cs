using System.Collections.Generic;
using Unleash.Variants;

namespace Unleash.Internal
{
    internal class FeatureToggle
    {
        public FeatureToggle(string name, bool enabled, List<ActivationStrategy> strategies, List<VariantDefinition> variants = null)
        {
            Name = name;
            Enabled = enabled;
            Strategies = strategies;
            Variants = variants ?? new List<VariantDefinition>();
        }

        public string Name { get; }
        public bool Enabled { get; }
        public List<ActivationStrategy> Strategies { get; }

        public List<VariantDefinition> Variants { get; }

        public override string ToString()
        {
            return $"FeatureToggle{{name=\'{Name}{'\''}, enabled={Enabled}, strategies=\'{Strategies}{'\''}{'}'}";
        }
    }
}