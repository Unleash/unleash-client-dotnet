using System;
using System.Collections.Generic;
using System.Text;
using Unleash.Internal;

namespace Unleash.Utilities
{
    public class ToggleBootstrapFileProvider : IToggleBootstrapProvider
    {
        private readonly string filePath;
        private readonly IFileSystem fileSystem;

        public ToggleBootstrapFileProvider(string filePath)
        {
            this.filePath = filePath;
            fileSystem = new FileSystem(Encoding.UTF8);
        }

        internal ToggleBootstrapFileProvider(string filePath, IFileSystem fileSystem)
        {
            this.filePath = filePath;
            this.fileSystem = fileSystem;
        }

        public string Read()
        {
            if (!fileSystem.FileExists(filePath))
                return string.Empty;

            return fileSystem.ReadAllText(filePath);
        }
    }
}
