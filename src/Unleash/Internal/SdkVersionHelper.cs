using System;
using System.Reflection;

namespace Unleash.Internal
{
    internal static class SdkVersionHelper
    {
        public static string SdkVersion => GetSdkVersion.Value;

        private static readonly Lazy<string> GetSdkVersion = new Lazy<string>(
            () =>
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                var version = assemblyName.Version.ToString(3);

                return $"v{version}";
            });
    }
}
