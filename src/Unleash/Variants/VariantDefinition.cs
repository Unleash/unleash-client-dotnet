using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Unleash.Internal;

namespace Unleash.Variants
{
    public class VariantDefinition
    {
        public string Name { get; set; }
        public int Weight { get; set; }
        public Payload Payload { get; set; }
        public ICollection<VariantOverride> Overrides { get; set; }
        public string Stickiness { get; set; }

        public VariantDefinition(string name, int weight, Payload payload = null, ICollection<VariantOverride> overrides = null, string stickiness = null)
        {
            Name = name;
            Weight = weight;
            Payload = payload;
            Overrides = overrides ?? new Collection<VariantOverride>();
            Stickiness = stickiness;
        }

        public Variant ToVariant()
        {
            return new Variant(Name, Payload, true);
        }
    }
}
