namespace Unleash
{
    using System;
    using System.Collections.Generic;

    public class FeatureToggle
    {
        public FeatureToggle(string name, bool enabled, List<ActivationStrategy> strategies)
        {
            Name = name;
            Enabled = enabled;
            Strategies = strategies;
        }

        public string Name { get; }
        public bool Enabled { get; }
        public List<ActivationStrategy> Strategies { get; }

        public override string ToString()
        {
            return $"FeatureToggle{{name=\'{Name}{'\''}, enabled={Enabled}, strategies=\'{Strategies}{'\''}{'}'}";
        }
    }
}