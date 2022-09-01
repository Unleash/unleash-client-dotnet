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
        private readonly string hostname;

        /// <inheritdoc />
        public ApplicationHostnameStrategy()
        {
            hostname = Environment.GetEnvironmentVariable("hostname") ?? Dns.GetHostName();
        }

        /// <inheritdoc />
        public string Name => NameConst;

        /// <inheritdoc />
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
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

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}