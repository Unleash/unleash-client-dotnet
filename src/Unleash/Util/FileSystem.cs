using System.IO;
using System.Text;

namespace Unleash.Util
{
    internal class FileSystem : IFileSystem
    {
        private readonly Encoding encoding;

        public FileSystem(UnleashConfig config)
        {
            encoding = config.Encoding;
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream FileOpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content, encoding);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, encoding);
        }
    }
}