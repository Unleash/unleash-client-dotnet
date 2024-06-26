using Unleash.Variants;

namespace Unleash.Internal
{
    public class Variant: Yggdrasil.Variant
    {
        public static new readonly Variant DISABLED_VARIANT = new Variant("disabled", null, false, false);
        
        public Variant(string name, Payload payload, bool enabled, bool fallback)
            : base(name, payload, enabled, fallback)
        {
        }
    }
}
