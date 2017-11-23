using System.IO;

namespace Unleash.Util
{
    internal interface IFileSystem
    {
        bool FileExists(string path);
        Stream FileOpenRead(string path);
        void WriteAllText(string path, string content);
        string ReadAllText(string path);
    }
}