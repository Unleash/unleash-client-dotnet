using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Internal;

namespace Unleash.Tests
{

    public class ExampleTests
    {
        private IUnleash unleash;

        [SetUp]
        public async Task Setup()
        {
            var factory = new ClientFactory.UnleashClientFactory(new MockedUnleashSettings());
            unleash = await factory.Generate(true);
        }

        [Test]
        public void UserAEnabled()
        {
            unleash.IsEnabled("one-enabled")
                .Should().BeTrue();
        }

        [Test]
        public void DisabledFeature()
        {
            unleash.IsEnabled("one-disabled")
                .Should().BeFalse();
        }

        [TearDown]
        public void Dispose()
        {
            unleash?.Dispose();
        }
    }
}