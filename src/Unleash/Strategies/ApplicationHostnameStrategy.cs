namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <inheritdoc />
    public class ApplicationHostnameStrategy : IStrategy
    {
        public static string HOST_NAMES_PARAM = "hostNames";
        protected string NAME = "applicationHostname";
        private string hostname;

        /// <inheritdoc />
        public ApplicationHostnameStrategy()
        {
            this.hostname = UnleashExtensions.GetLocalIpAddress();
        }

        /// <inheritdoc />
        public string Name => NAME;

        /// <inheritdoc />
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            if (parameters.TryGetValue(HOST_NAMES_PARAM, out var hostnames))
            {
                if (hostnames == null)
                    return false;

                return hostnames
                    .ToLowerInvariant()
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Contains(hostname.ToLowerInvariant());
            }

            return false;
        }
    }
}