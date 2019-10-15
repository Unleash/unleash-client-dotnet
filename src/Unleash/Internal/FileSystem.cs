using System.IO;
using System.Text;

namespace Unleash.Internal
{
    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream FileOpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream FileOpenCreate(string path)
        {
            return File.Open(path, FileMode.Create);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}
