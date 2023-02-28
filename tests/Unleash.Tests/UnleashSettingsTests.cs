using FluentAssertions;
using NUnit.Framework;

namespace Unleash.Tests
{
    public class UnleashSettingsTests
    {
        [Test]
        public void Should_set_environment_to_default()
        {
            // Act
            var settings = new UnleashSettings();

            // Assert
            settings.Environment.Should().Be("default");
        }

        [Test]
        public void Should_set_sdk_name()
        {
            // Act
            var settings = new UnleashSettings();

            // Assert
            settings.SdkVersion.Should().StartWith("unleash-client-dotnet:v");
        }
    }
}