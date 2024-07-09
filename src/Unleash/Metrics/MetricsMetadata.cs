using System.Text.RegularExpressions;

internal static class MetricsMetadata
{
    internal static string GetPlatformName()
    {
#if NETSTANDARD1_1_OR_GREATER || NETCOREAPP1_0_OR_GREATER
        return GetPlatformName(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
#else
            return "PreDotNetCore";
#endif
    }

    internal static string GetPlatformName(string frameworkDescription)
    {
        var match = Regex.Match(frameworkDescription, @"^(?<name>.+?) \d");
        return match.Success ? match.Groups["name"].Value : frameworkDescription;
    }

    internal static string GetPlatformVersion()
    {
#if NETSTANDARD1_1_OR_GREATER || NETCOREAPP1_0_OR_GREATER
        return GetPlatformVersion(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
#else
            return null;
#endif
    }

    internal static string GetPlatformVersion(string frameworkDescription)
    {
        var match = Regex.Match(frameworkDescription, @"(\d+\.\d+\.\d+)");
        return match.Success ? match.Value : null;
    }
}