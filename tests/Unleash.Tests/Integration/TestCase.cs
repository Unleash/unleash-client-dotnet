namespace Unleash.Tests.Specifications
{
    public class TestCase
    {
        public string Description { get; set; }
        public UnleashContextDefinition Context { get; set; }
        public string ToggleName { get; set; }
        public bool ExpectedResult { get; set; }
    }
}
