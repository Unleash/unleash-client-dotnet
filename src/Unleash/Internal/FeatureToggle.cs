using System.Collections.Generic;

namespace Unleash.Internal
{
    internal class FeatureToggle
    {
        public FeatureToggle(string name, bool enabled, List<ActivationStrategy> strategies, List<Variants> variants = null)
        {
            Name = name;
            Enabled = enabled;
            Strategies = strategies;
            Variants = variants;
        }

        public string Name { get; }
        public bool Enabled { get; }
        public List<ActivationStrategy> Strategies { get; }

        public List<Variants> Variants { get; }

        public override string ToString()
        {
            return $"FeatureToggle{{name=\'{Name}{'\''}, enabled={Enabled}, strategies=\'{Strategies}{'\''}{'}'}";
        }
    }
}