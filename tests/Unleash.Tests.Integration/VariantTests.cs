using System.Collections.Generic;
using System.Linq;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Unleash.Tests.Integration.Fixtures;
using Xunit;

namespace Unleash.Tests.Integration
{
    public class VariantTests : BaseUnleashServiceIntegrationTests
    {
        public VariantTests(UnleashServiceFixture unleashServiceFixture) : base(unleashServiceFixture)
        {
        }

        [Fact]
        public void GetVariants_WithExpectedData_ReturnsExpectedResults()
        {
            var expectedVariants = new List<Variant>()
            {
                new Variant(
                    "variant.1",
                    34,
                    new Payload
                    {
                        Type = "string",
                        Value = "variant 1 payload"
                    },
                    null),

                new Variant(
                    "variant.2",
                    33,
                    new Payload
                    {
                        Type = "string",
                        Value = "variant 2 payload"
                    },
                    null),

                new Variant(
                    "variant.3",
                    33,
                    new Payload
                    {
                        Type = "string",
                        Value = "variant 3 payload"
                    },
                    null)
            };

            var actualVariants= Unleash.GetVariants("unleash.client.test.integration.flag.enabled.with-three-variants").ToArray();

            Assert.Equal(3, actualVariants.Length);
            AssertionUtils.AssertVariantsEquivalent(expectedVariants, actualVariants);
        }
    }
}
