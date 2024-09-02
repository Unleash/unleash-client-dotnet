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
        private readonly UnleashSettings settings;

        internal ToggleBootstrapFileProvider(string filePath, UnleashSettings settings)
        {
            this.filePath = filePath;
            this.settings = settings;
        }

        [Obsolete("Will return json string in the next major version", false)]
        public ToggleCollection Read()
        {
            using (var togglesStream = settings.FileSystem.FileOpenRead(filePath))
                return settings.JsonSerializer.Deserialize<ToggleCollection>(togglesStream);
        }
    }
}
