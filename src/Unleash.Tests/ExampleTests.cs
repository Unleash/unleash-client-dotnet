using System;
using FluentAssertions;
using NUnit.Framework;

namespace Unleash.Tests
{
    public class ExampleTests : IDisposable
    {
        private readonly IUnleash unleash;

        public ExampleTests()
        {
            var config = new UnleashTestConfig();
            unleash = new DefaultUnleash(config);
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

        public void Dispose()
        {
            unleash?.Dispose();
        }
    }
}