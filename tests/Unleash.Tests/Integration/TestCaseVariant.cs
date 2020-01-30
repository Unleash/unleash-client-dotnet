using Unleash.Internal;

namespace Unleash.Tests.Specifications
{
    public class TestCaseVariant
    {
        private Variant _expectedResult;

        public string Description { get; set; }
        public UnleashContextDefinition Context { get; set; }
        public string ToggleName { get; set; }
        public Variant ExpectedResult {
            set => _expectedResult = value;
            get => _expectedResult?.Name.Equals("disabled") == true ? Variant.DISABLED_VARIANT : _expectedResult;
        }
    }
}
