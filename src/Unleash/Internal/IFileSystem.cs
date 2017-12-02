using System.IO;

namespace Unleash.Internal
{
    internal interface IFileSystem
    {
        bool FileExists(string path);
        Stream FileOpenRead(string path);
        Stream FileOpenCreate(string path);
        void WriteAllText(string path, string content);
        string ReadAllText(string path);
    }
}