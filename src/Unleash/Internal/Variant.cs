using Unleash.Variants;

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
    }
}
