using System.IO;
using Unleash.Util;

namespace Unleash.Tests.TestHelpers
{
    class MockFileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            throw new System.NotImplementedException();
        }

        public Stream FileOpenRead(string path)
        {
            throw new System.NotImplementedException();
        }

        public void WriteAllText(string path, string content)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}