using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public void GetExistingVariantsOfActiveTooggle()
        {
            var list = new List<Variants>();
            list.Add(new Variants("Aa", 50, null));

            var variants = _unleash.GetVariants("one-enabled", "Aa");

            variants.Should().HaveCount(1);
            list.ShouldBeEquivalentTo(variants);
        }

        [Test]
        public void GetUnexistingVariantsOfActiveTooggle()
        {
            var variants = _unleash.GetVariants("one-enabled", "XX");

            variants.Should().BeNull();
        }

        [Test]
        public void GetVariantsOfUnexistingTooggle()
        {
            var variants = _unleash.GetVariants("XX", "XX");

            variants.Should().BeNull();
        }

        [Test]
        public void GetVariantsOfInactiveTooggle()
        {
            var variants = _unleash.GetVariants("one-disabled", "XX");

            variants.Should().BeNull();
        }
    }
}
