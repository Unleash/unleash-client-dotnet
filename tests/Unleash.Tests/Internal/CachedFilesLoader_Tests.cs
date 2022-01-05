using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.Text;
using Unleash.Internal;
using Unleash.Tests.Serialization;

namespace Unleash.Tests.Internal
{
    public class CachedFilesLoader_Tests
    {
        protected string AppDataFile(string filename)
        {
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, "App_Data", filename);
            return file;
        }

        [Test]
        public void Sets_Etag_From_Etag_File_When_Backup_Is_Not_Empty()
        {
            string toggleFileName = AppDataFile("unleash-repo-v1.json");
            string etagFileName = AppDataFile("etag-12345.txt");
            var serializer = new JsonNetSerializer();
            var fileSystem = new FileSystem(Encoding.UTF8);

            var fileLoader = new CachedFilesLoader(serializer, fileSystem, toggleFileName, etagFileName);
            var ensureResult = fileLoader.EnsureExistsAndLoad();
            ensureResult.InitialETag.Should().Be("12345");
        }
    }
}
