using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Unleash.Logging;

namespace Unleash
{
    internal static class UnleashExtensions
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(UnleashExtensions));

        internal static string ConvertToString(this Stream stream)
        {
            stream.Position = 0;

            using (stream)
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        internal static Stream ConvertToStream(this string s)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 4, true))
            {
                writer.Write(s);
                writer.Flush();
            }
            stream.Position = 0;
            return stream;
        }

        internal static void SetRequestProperties(this HttpRequestMessage requestMessage, UnleashConfig config)
        {
            const string appNameHeader = "UNLEASH-APPNAME";
            const string instanceIdHeader = "UNLEASH-INSTANCEID";
            const string userAgentHeader = "User-Agent";

            requestMessage.Headers.TryAddWithoutValidation(appNameHeader, config.AppName);
            requestMessage.Headers.TryAddWithoutValidation(instanceIdHeader, config.InstanceId);
            requestMessage.Headers.TryAddWithoutValidation(userAgentHeader, config.AppName);

            if (config.CustomHttpHeaders.Count == 0)
                return;

            foreach (var header in config.CustomHttpHeaders)
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        internal static string GetLocalIpAddress()
        {
            try
            {
                var hostname = Environment.GetEnvironmentVariable("hostname");
                if (hostname != null)
                    return hostname;

                var host = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                return "undefined-ip-address";
            }
            catch (Exception exception)
            {
                Logger.TraceException("UNLEASH: Failed to extract local ip address", exception);
                return "undefined";
            }
        }
    }
}