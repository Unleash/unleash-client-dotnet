using System;
using System.Collections.Generic;
using Unleash.Variants;

namespace Unleash.Internal
{
    public class FeatureEvaluationResult
    {
        public bool Enabled { get; set; }
        public Variant Variant { get; set; }
    }
}

