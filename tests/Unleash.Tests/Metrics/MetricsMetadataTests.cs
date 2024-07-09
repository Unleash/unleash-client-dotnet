using FluentAssertions;
using NUnit.Framework;

public class MetricsMetadataTests
{
    [TestCase(".NET Core 3.1.32", ".NET Core", "3.1.32")]
    [TestCase(".NET 7.0.12", ".NET", "7.0.12")]
    [TestCase(".NET", ".NET", null)] //apparently this can be a thing but the docs are very shady on when
    public void GetPlatformName_ShouldReturnDotNet(string inputString, string expectedName, string expectedVersion)
    {
        var platformName = MetricsMetadata.GetPlatformName(inputString);
        var platformVersion = MetricsMetadata.GetPlatformVersion(inputString);

        platformName.Should().Be(expectedName);
        platformVersion.Should().Be(expectedVersion);
    }
}