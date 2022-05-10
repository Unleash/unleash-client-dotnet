using System;
using System.Collections.Generic;
using System.Text;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Utilities
{
    public class ToggleBootstrapFileProvider : IToggleBootstrapProvider
    {
        private readonly string filePath;
        private readonly IFileSystem fileSystem;
        private readonly IJsonSerializer jsonSerializer;


        internal ToggleBootstrapFileProvider(string filePath, IFileSystem fileSystem, IJsonSerializer jsonSerializer)
        {
            this.filePath = filePath;
            this.fileSystem = fileSystem;
            this.jsonSerializer = jsonSerializer;
        }

        public ToggleCollection Read()
        {
            using (var togglesStream = fileSystem.FileOpenRead(filePath))
                return jsonSerializer.Deserialize<ToggleCollection>(togglesStream);
        }
    }
}
