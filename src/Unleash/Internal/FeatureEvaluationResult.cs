using System;
using System.Collections.Generic;
using Unleash.Variants;

namespace Unleash.Internal
{
    public class FeatureEvaluationResult
    {
        public List<VariantDefinition> StrategyVariants { get; set; }
        public bool Enabled { get; set; }
    }
}

