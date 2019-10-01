using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class Variant
    {
        public Variant(string name, int weight, Payload payload, ICollection<Override> overrides)
        {
            Name = name;
            Weight = weight;
            Payload = payload;
            Overrides = overrides;
        }

        public string Name { get; set; }
        public int Weight { get; set; }
        public Payload Payload { get; set; }
        public ICollection<Override> Overrides { get; set; }
    }
}
