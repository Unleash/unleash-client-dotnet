using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Tests.Serialization;

namespace Unleash.Tests.Internal
{
    public class ToggleBootstrapHandler_Tests : CachedFilesLoaderTestBase
    {
        [Test]
        public void Loads_From_Bootstrap_Provider()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var toggleProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => toggleProviderFake.Read())
                .Returns(fileSystem.ReadAllText(toggleFileName));

            var settings = new UnleashSettings()
            {
                JsonSerializer = serializer,
                FileSystem = fileSystem,
                ToggleBootstrapProvider = toggleProviderFake
            };

            var bootstrapHandler = new ToggleBootstrapHandler(settings);

            // Act
            var toggleCollection = bootstrapHandler.Read();

            // Assert
            A.CallTo(() => toggleProviderFake.Read())
                .MustHaveHappenedOnceExactly();
            toggleCollection.Should().NotBeNull();
            toggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Returns_Null_When_Provider_In_Settings_Is_Null()
        {
            // Arrange
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);

            var settings = new UnleashSettings()
            {
                JsonSerializer = serializer,
                FileSystem = fileSystem,
            };

            var bootstrapHandler = new ToggleBootstrapHandler(settings);

            // Act
            var toggleCollection = bootstrapHandler.Read();

            // Assert
            toggleCollection.Should().BeNull();
        }
    }
}
