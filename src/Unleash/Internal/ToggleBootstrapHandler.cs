using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unleash.Serialization;

namespace Unleash.Internal
{
    internal class ToggleBootstrapHandler : IToggleBootstrapHandler
    {
        private readonly IJsonSerializer jsonSerializer;
        private readonly IFileSystem fileSystem;
        private readonly IToggleBootstrapProvider toggleBootstrapProvider;
        private readonly Encoding encoding;

        public ToggleBootstrapHandler(UnleashSettings settings)
        {
            jsonSerializer = settings.JsonSerializer;
            fileSystem = settings.FileSystem;
            toggleBootstrapProvider = settings.ToggleBootstrapProvider;
            encoding = settings.Encoding;
        }

        public ToggleCollection Read()
        {
            if (toggleBootstrapProvider == null)
                return null;

            var bootstrapContent = toggleBootstrapProvider.Read();
            var bootstrapContentBuffer = encoding.GetBytes(bootstrapContent ?? "");
            using (var bootstrapStream = new MemoryStream(bootstrapContentBuffer))
            {
                return jsonSerializer.Deserialize<ToggleCollection>(bootstrapStream);
            }
        }
    }
}
