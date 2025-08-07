using System;
using System.IO;
using System.Text;

namespace Unleash.Internal
{
    internal class FileSystem : IFileSystem
    {
        private readonly Encoding encoding;

        public Encoding Encoding => encoding;

        public FileSystem(Encoding encoding)
        {
            this.encoding = encoding;
        }

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
            var tempFile = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            var backupFile = path + ".bak";

            try
            {
                File.WriteAllText(tempFile, content, encoding);

                if (File.Exists(path))
                {
                    File.Replace(tempFile, path, backupFile, ignoreMetadataErrors: true);
                    TryDelete(backupFile);
                }
                else
                {
                    File.Move(tempFile, path);
                }
            }
            finally
            {
                TryDelete(tempFile);
            }
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, encoding);
        }

        private void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch {  /* Intentionally swallowed, there's nothing we can or need to do here. Temp files will eventually be claimed by the OS */ }
        }
    }
}