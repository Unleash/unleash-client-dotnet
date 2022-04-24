using System.Collections.Generic;
using Unleash.Variants;

namespace Unleash.Internal
{
    public class FeatureToggle
    {
        public FeatureToggle(
            string name,
            string description,
            string type,
            bool enabled,
            bool stale,
            List<ActivationStrategy> strategies,
            List<VariantDefinition> variants = null)
        {
            Name = name;
            Description = description;
            Type = type;
            Enabled = enabled;
            Stale = stale;
            Strategies = strategies;
            Variants = variants ?? new List<VariantDefinition>();
        }

        public string Name { get; }
        public string Description { get; }
        public string Type { get; }
        public bool Enabled { get; }
        public bool Stale { get; }
        public List<ActivationStrategy> Strategies { get; }

        public List<VariantDefinition> Variants { get; }

        public override string ToString()
        {
            return $"FeatureToggle{{name=\'{Name}{'\''}, enabled={Enabled}, stale={Stale}, strategies=\'{Strategies}{'\''}{'}'}";
        }
    }
}