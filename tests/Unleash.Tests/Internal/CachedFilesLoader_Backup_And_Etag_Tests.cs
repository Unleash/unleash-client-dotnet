using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.Text;
using Unleash.Internal;
using Unleash.Tests.Serialization;

namespace Unleash.Tests.Internal
{
    public class CachedFilesLoader_Backup_And_Etag_Tests : CachedFilesLoaderTestBase
    {
        [Test]
        public void Sets_Etag_From_Etag_File_And_Toggles_From_Backup_When_Backup_Is_Not_Empty()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be("12345");
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Writes_Empty_Etag_File_And_Sets_Etag_To_Empty_String_And_Loads_Toggles_When_Etag_File_Is_Missing_But_Backup_Exists()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be(string.Empty);
            fileSystem.FileExists(etagFileName).Should().BeTrue();
            fileSystem.ReadAllText(etagFileName).Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Features.Should().HaveCount(3);
        }

        [Test]
        public void Sets_Etag_To_Empty_String_And_Toggles_To_Null_When_Etag_File_Exists_But_Backup_File_Does_Not()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-missing.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be(string.Empty);
            ensureResult.InitialToggleCollection.Should().BeNull();
            fileSystem.FileExists(toggleFileName).Should().BeTrue();
            fileSystem.ReadAllText(toggleFileName).Should().Be(string.Empty);
        }

        [Test]
        public void Sets_Etag_To_Empty_String_And_Toggles_To_Null_When_Neither_File_Exists()
        {
            // Arrange
            string toggleFileName = AppDataFile("unleash-repo-missing.json");
            string etagFileName = AppDataFile("etag-missing.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);
            var settings = new UnleashSettings();
            var fileLoader = new CachedFilesLoader(serializer, fileSystem, null, toggleFileName, etagFileName);

            // Act
            var ensureResult = fileLoader.EnsureExistsAndLoad();

            // Assert
            ensureResult.InitialETag.Should().Be(string.Empty);
            fileSystem.FileExists(etagFileName).Should().BeTrue();
            fileSystem.ReadAllText(etagFileName).Should().Be(string.Empty);

            ensureResult.InitialToggleCollection.Should().BeNull();
            fileSystem.FileExists(toggleFileName).Should().BeTrue();
            fileSystem.ReadAllText(toggleFileName).Should().Be(string.Empty);
        }
    }
}
