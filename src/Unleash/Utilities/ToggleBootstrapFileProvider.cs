﻿using Unleash.Internal;

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

        public string Read()
        {
            return settings.FileSystem.ReadAllText(filePath);
        }
    }
}
