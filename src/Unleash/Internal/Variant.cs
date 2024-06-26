using Unleash.Variants;

namespace Unleash.Internal
{
    public class Variant: Yggdrasil.Variant
    {
        public Variant(string name, Payload payload, bool enabled, bool fallback)
            : base(name, payload, enabled, fallback)
        {
        }
    }
}
