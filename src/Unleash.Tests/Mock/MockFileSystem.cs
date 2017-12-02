using System.IO;
using Unleash.Internal;

namespace Unleash.Tests.Mock
{
    class MockFileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return true;
        }

        public Stream FileOpenRead(string path)
        {
            return new MemoryStream();
        }

        public Stream FileOpenCreate(string path)
        {
            return new MemoryStream();
        }

        public void WriteAllText(string path, string content)
        {
        }

        public string ReadAllText(string path)
        {
            return "mock";
        }
    }
}