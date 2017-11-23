namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RemoteAddressStrategy : IStrategy
    {
        internal static readonly string PARAM = "IPs";

        public string Name => "remoteAddress";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            var remoteAddress = context?.RemoteAddress;
            if (remoteAddress == null)
                return false;

            if (parameters.TryGetValue(PARAM, out var remoteAddresses))
            {
                return remoteAddresses
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Contains(remoteAddress);
            }

            return false;
        }
    }
}