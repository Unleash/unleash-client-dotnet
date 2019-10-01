using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unleash.Internal;

namespace Unleash.Tests.Basic
{
    public class VariantsTest : IDisposable
    {
        private readonly IUnleash _unleash;

        public VariantsTest()
        {
            _unleash = new DefaultUnleash(new MockedUnleashSettings());
        }

        public void Dispose()
        {
            _unleash?.Dispose();
        }

        [Test]
        public void GetExistingVariantsOfActiveToggle()
        {
            var expected = new List<Variant>()
            {
                new Variant("Aa", 33, null, null),
                new Variant("Aa", 33, null, null),
            };

            var variants = _unleash.GetVariants("one-enabled", "Aa");

            variants
                .Should()
                .HaveCount(2);
            expected
                .ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void GetUnExistingVariantsOfActiveToggle()
        {
            var variants = _unleash.GetVariants("one-enabled", "XX");

            variants.Should().BeNull();
        }

        [Test]
        public void GetVariantsOfUnExistingToggle()
        {
            var variants = _unleash.GetVariants("XX", "XX");

            variants.Should().BeNull();
        }

        [Test]
        public void GetVariantsOfInactiveToggle()
        {
            var variants = _unleash.GetVariants("one-disabled", "XX");

            variants.Should().BeNull();
        }

        [Test]
        public void GetAllVariantsFromInactiveToggle()
        {
            var variants = _unleash.GetVariants("one-disabled");
            
            variants.Should().BeNull();
        }

        [Test]
        public void GetAllVariantsFromActiveToggle()
        {
            var variants = _unleash.GetVariants("one-enabled");

            variants.Should().HaveCount(3);
        }

        [Test]
        public void GetWeightedVariantFromInactiveToggle()
        {
            var variant = _unleash.GetVariant("one-disabled");

            variant.Should().BeNull();
        }
        
        [Test]
        public void GetWeightedVariantFromActiveToggle()
        {
            var variants = _unleash.GetVariants("one-enabled");
            var variant = _unleash.GetVariant("one-enabled");

            variant.Should().NotBeNull();
            variants.Should().Contain(variant);
        }
    }
}
