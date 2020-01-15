using Unleash.Variants;

namespace Unleash.Internal
{
    public class Variant
    {
        public static readonly Variant DISABLED_VARIANT = new Variant("disabled", null, false);

        public Variant(string name, Payload payload, bool enabled)
        {
            Name = name;
            Payload = payload;
            IsEnabled = enabled;
        }

        public string Name { get; }
        public Payload Payload { get; }
        public bool IsEnabled { get; }
    }
}
