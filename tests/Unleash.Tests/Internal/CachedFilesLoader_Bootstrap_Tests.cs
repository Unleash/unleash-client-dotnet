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
                new FeatureToggle("one-enabled",  "release", true, false, new List<ActivationStrategy>()
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
                new FeatureToggle("one-disabled",  "release", false, false, new List<ActivationStrategy>()
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
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => bootstrapProviderFake.Read())
                .Returns(bootstrapToggles);

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapProviderFake.Read())
                .MustHaveHappenedOnceExactly();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(2);
        }

        [Test]
        public void Loads_From_Bootstrap_Provider_When_Backup_File_Is_Missing_And_Returns_Null_When_Bootstrap_File_Returns_Null()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1-missing.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            ToggleCollection bootstrapToggles = null;
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => bootstrapProviderFake.Read())
                .Returns(bootstrapToggles);

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapProviderFake.Read())
                .MustHaveHappenedOnceExactly();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Should().BeNull();
        }

        [Test]
        public void Default_Override_Calls_Bootstrap_Handler_When_Backup_File_Exists()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var bootstrapToggles = GetTestToggles();
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => bootstrapProviderFake.Read())
                .Returns(bootstrapToggles);

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapProviderFake.Read())
                .MustHaveHappened();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(2);
        }

        [Test]
        public void Does_Not_Call_Bootstrap_Handler_When_Backup_File_Exists_And_Override_Is_False()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var bootstrapToggles = GetTestToggles();
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName, false);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            A.CallTo(() => bootstrapProviderFake.Read())
                .MustNotHaveHappened();
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Default_Override_Null_Should_Not_Null_Out_Backup_Toggles()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, null, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be("12345");
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Default_Override_Should_Not_Null_Out_Backup_Toggles_When_Bootstrap_Result_Is_Null()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => bootstrapProviderFake.Read())
                .Returns(null);
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName, true);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be("12345");
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Default_Override_Should_Not_Override_Backup_Toggles_When_Bootstrap_Result_Is_Empty_Collection()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => bootstrapProviderFake.Read())
                .Returns(new ToggleCollection());
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName, true);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be("12345");
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Bootstrap_Override_Disabled_Bootstraps_When_Backup_Is_Empty_Collection()
        {
            // Arrange
            string toggleFileName = AppDataFile("features-v1-empty.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var bootstrapToggles = GetTestToggles();
            var bootstrapProviderFake = A.Fake<IToggleBootstrapProvider>();
            A.CallTo(() => bootstrapProviderFake.Read())
                .Returns(bootstrapToggles);
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, bootstrapProviderFake, null, toggleFileName, etagFileName, false);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be("12345");
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(2);
        }
    }
}
