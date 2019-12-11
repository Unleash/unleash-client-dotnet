using AutoFixture.Xunit2;

namespace Unleash.Core.Tests.Utility
{
    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] values)
            : base(new AutoMoqDataAttribute(), values) { }
    }
}
