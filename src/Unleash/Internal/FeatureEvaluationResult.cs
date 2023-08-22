using System;
using System.Collections.Generic;
using Unleash.Variants;

namespace Unleash.Internal
{
    internal class FeatureEvaluationResult
    {
        public bool Enabled { get; set; }
        public Variant Variant { get; set; }
    }
}

