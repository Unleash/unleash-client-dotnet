using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Utilities;

namespace Unleash.Tests.Utilities
{
    public class ToggleBootstrapFileProvider_Tests
    {
        protected string AppDataFile(string filename)
        {
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, "App_Data", filename);
            return file;
        }

        [Test]
        public void Returns_String_Empty_When_File_Does_Not_Exist()
        {
            // Arrange
            var fileSystem = new FileSystem(Encoding.UTF8);
            string toggleFileName = AppDataFile("unleash-repo-v1-missing.json");
            var toggleFileProvider = new ToggleBootstrapFileProvider(toggleFileName, fileSystem);

            // Act
            var emptyResult = toggleFileProvider.Read();

            // Assert
            emptyResult.Should().Equals(string.Empty);
        }

        [Test]
        public void Returns_File_Content_When_File_Exists()
        {
            // Arrange
            var fileSystem = new FileSystem(Encoding.UTF8);
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            var toggleFileProvider = new ToggleBootstrapFileProvider(toggleFileName, fileSystem);
            var fileContent = fileSystem.ReadAllText(toggleFileName);

            // Act
            var result = toggleFileProvider.Read();

            // Assert
            result.Should().Equals(fileContent);
        }
    }
}
