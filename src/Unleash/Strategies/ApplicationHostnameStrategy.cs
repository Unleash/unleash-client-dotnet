namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ApplicationHostnameStrategy : IStrategy
    {
        public static string HOST_NAMES_PARAM = "hostNames";
        protected string NAME = "applicationHostname";
        private string hostname;

        public ApplicationHostnameStrategy()
        {
            this.hostname = UnleashExtensions.GetLocalIpAddress();
        }

        public string Name => NAME;

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