using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Utilities
{
    public class ToggleBootstrapFileProvider : IToggleBootstrapProvider
    {
        private readonly string filePath;
        private readonly UnleashSettings settings;

        internal ToggleBootstrapFileProvider(string filePath, UnleashSettings settings)
        {
            this.filePath = filePath;
            this.settings = settings;
        }

        public BootstrapLoadResult Read()
        {
            var fileContent = settings.FileSystem.ReadAllText(filePath);
            using (var togglesStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
                return new BootstrapLoadResult {
                    ToggleCollection = settings.JsonSerializer.Deserialize<ToggleCollection>(togglesStream)
                };
        }
    }
}
