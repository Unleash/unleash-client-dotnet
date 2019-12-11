using System.IO;
using System.Linq;
using Unleash.Internal;

namespace Unleash
{
    public static class UnleashSettingsExtensions
    {
        public static string GetFeatureToggleFilePath(this UnleashSettings unleashSettings)
        {
            return Path.Combine(unleashSettings.LocalStorageFolder, unleashSettings.PrependFileName(unleashSettings.FeatureToggleFilename));
        }

        public static string GetFeatureToggleETagFilePath(this UnleashSettings unleashSettings)
        {
            return Path.Combine(unleashSettings.LocalStorageFolder, unleashSettings.PrependFileName(unleashSettings.EtagFilename));
        }

        private static string PrependFileName(this UnleashSettings unleashSettings, string filename)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            var extension = Path.GetExtension(filename);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            return new string($"{fileNameWithoutExtension}-{unleashSettings.AppName}-{unleashSettings.InstanceTag}-{SdkVersionHelper.SdkVersion}{extension}"
                .Where(c => !invalidFileNameChars.Contains(c))
                .ToArray());
        }
    }
}
