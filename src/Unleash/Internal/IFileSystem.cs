using System.IO;

namespace Unleash.Internal
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        Stream FileOpenRead(string path);
        Stream FileOpenCreate(string path);
        void WriteAllText(string path, string content);
        string ReadAllText(string path);
    }
}
