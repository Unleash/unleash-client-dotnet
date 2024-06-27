using Unleash.Variants;

namespace Unleash.Internal
{
    public class Variant: Yggdrasil.Variant
    {
        public static new readonly Variant DISABLED_VARIANT = new Variant("disabled", null, false, false);
        
        public Variant(string name, Yggdrasil.Payload payload, bool enabled, bool fallback)
            : base(name, payload, enabled, fallback)
        {
        }

        internal static Variant UpgradeVariant(Yggdrasil.Variant variant)
        {
            return new Variant(variant.Name, variant.Payload, variant.IsEnabled, variant.FeatureEnabled);
        }
    }
}
