using Unleash.Variants;
using YggdrasilVariant = Yggdrasil.Variant;

namespace Unleash.Internal
{
    public class Variant
    {
        public static readonly Variant DISABLED_VARIANT = new Variant("disabled", null, false, false);

        public Variant(string name, Payload payload, bool enabled, bool feature_enabled)
        {
            Name = name;
            Payload = payload;
            IsEnabled = enabled;
            FeatureEnabled = feature_enabled;
        }

        public string Name { get; }
        public Payload Payload { get; }
        public bool IsEnabled { get; }
        public bool FeatureEnabled { get; internal set; }

        public static Variant FromEngineVariant(YggdrasilVariant variant)
        {
            if (variant == null)
                return DISABLED_VARIANT;

            var payload = variant.Payload != null ? new Payload(variant.Payload.PayloadType, variant.Payload.Value) : null;

            return new Variant(variant.Name, payload, variant.Enabled, variant.FeatureEnabled);
        }
    }
}
