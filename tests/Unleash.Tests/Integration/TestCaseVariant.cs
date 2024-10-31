using Unleash.Internal;

namespace Unleash.Tests.Specifications
{
    public class TestCaseVariant
    {
        public string Description { get; set; }
        public UnleashContextDefinition Context { get; set; }
        public string ToggleName { get; set; }
        public Variant ExpectedResult { get; set; }
    }
}
