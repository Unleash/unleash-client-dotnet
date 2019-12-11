using Unleash.Internal;

namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <inheritdoc />
    public class ApplicationHostnameStrategy : IStrategy
    {
        public static string HostNamesParam = "hostNames";

        protected readonly string NameConst = "applicationHostname";

        /// <inheritdoc />
        public string Name => NameConst;

        /// <inheritdoc />
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context)
        {
            var hostname = Environment.GetEnvironmentVariable("hostname") ?? Dns.GetHostName();

            if (parameters.TryGetValue(HostNamesParam, out var hostnames))
            {
                if (hostnames == null || hostnames == string.Empty)
                    return false;

                return hostnames
                    .ToLowerInvariant()
                    .Split(',')
                    .Select(x => x.Trim())
                    .Contains(hostname.ToLowerInvariant());
            }

            return false;
        }
    }
}
