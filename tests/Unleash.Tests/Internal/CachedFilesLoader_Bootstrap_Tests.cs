using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unleash.Internal;
using Unleash.Tests.Mock;
using Unleash.Tests.Serialization;
using Unleash.Variants;

namespace Unleash.Tests.Internal
{
    public class CachedFilesLoader_Bootstrap_Tests : CachedFilesLoaderTestBase
    {
        private static ToggleCollection GetTestToggles()
        {
            return new ToggleCollection(new List<FeatureToggle>
            {
                new FeatureToggle("one-enabled",  "release", true, new List<ActivationStrategy>()
                {
                    new ActivationStrategy("userWithId", new Dictionary<string, string>(){
                        {"userIds", "userA" }
                    })
                }, new List<VariantDefinition>()
                {
                    new VariantDefinition("Aa", 33, null, null),
                    new VariantDefinition("Aa", 33, null, null),
                    new VariantDefinition("Ab", 34, null, new List<VariantOverride>{ new VariantOverride("context", new[] { "a", "b"}) }),
                }
                ),
                new FeatureToggle("one-disabled",  "release", false, new List<ActivationStrategy>()
                {
                    new ActivationStrategy("userWithId", new Dictionary<string, string>()
                    {
                        {"userIds", "userB" }
                    })
                })
            });
        }

        [Test]
        public void Loads_From_Bootstrap_Provider_When_Backup_File_Is_Missing()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1-missing.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var bootstrapToggles = GetTestToggles();
            var bootstrapHandlerFake = A.Fake<IToggleBootstrapHandler>();
            A.CallTo(() => bootstrapHandlerFake.Read())
                .Returns(bootstrapToggles);

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapHandlerFake, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapHandlerFake.Read())
                .MustHaveHappenedOnceExactly();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(2);
        }

        [Test]
        public void Loads_From_Bootstrap_Handler_When_Backup_File_Is_Missing_And_Returns_Null_When_Bootstrap_File_Returns_Null()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1-missing.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            ToggleCollection bootstrapToggles = null;
            var bootstrapHandlerFake = A.Fake<IToggleBootstrapHandler>();
            A.CallTo(() => bootstrapHandlerFake.Read())
                .Returns(bootstrapToggles);

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapHandlerFake, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapHandlerFake.Read())
                .MustHaveHappenedOnceExactly();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Should().BeNull();
        }

        [Test]
        public void Does_Not_Call_Bootstrap_Handler_When_Backup_File_Exists()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var bootstrapToggles = GetTestToggles();
            var bootstrapHandlerFake = A.Fake<IToggleBootstrapHandler>();

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapHandlerFake, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapHandlerFake.Read())
                .MustNotHaveHappened();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }
    }
}
