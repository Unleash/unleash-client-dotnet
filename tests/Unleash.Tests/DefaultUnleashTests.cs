using FluentAssertions;
using NUnit.Framework;
using System;
using Unleash.Tests.Mock;

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

        [Test]
        public void Configure_Http_Client_Factory()
        {
            // Arrange
            var factory = new HttpClientFactoryMock();
            var apiUri = new Uri("http://localhost:8080/");

            // Act
            var client = factory.Create(apiUri);

            // Assert
            factory.CreateHttpClientInstanceCalled.Should().BeTrue();
        }
    }
}
