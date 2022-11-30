using FluentAssertions;
using NUnit.Framework;
using System;

namespace Unleash.Tests
{
    public class DefaultUnleashTests
    {
        [Test]
        public void ConfigureEvents_should_invoke_callback()
        {
            // Arrange
            var settings = new UnleashSettings
            {
                AppName = "testapp",
            };

            var unleash = new DefaultUnleash(settings);
            var callbackCalled = false;

            // Act
            unleash.ConfigureEvents(cfg =>
            {
                callbackCalled = true;
            });

            // Assert
            callbackCalled.Should().BeTrue();
        }
    }
}
