namespace Unleash.Internal
{
    public class Variant : Yggdrasil.Variant
    {
        public static new readonly Variant DISABLED_VARIANT = UpgradeVariant(Yggdrasil.Variant.DISABLED_VARIANT);

        public Variant(string name, Yggdrasil.Payload payload, bool enabled, bool feature_enabled)
            : base(name, payload, enabled, feature_enabled)
        {
        }

        internal static Variant UpgradeVariant(Yggdrasil.Variant variant)
        {
            return new Variant(variant.Name, variant.Payload, variant.Enabled, variant.FeatureEnabled);
        }
    }
}
