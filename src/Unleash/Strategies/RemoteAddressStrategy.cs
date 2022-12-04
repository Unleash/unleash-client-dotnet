namespace Unleash.Strategies
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Unleash.Internal;
    using Unleash.Utilities;

    public class RemoteAddressStrategy : IStrategy
    {
        internal static readonly string PARAM = "IPs";

        public string Name => "remoteAddress";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            var remoteAddress = context?.RemoteAddress;
            IPAddress remoteIPAddress;

            if (string.IsNullOrEmpty(remoteAddress) || !IPAddress.TryParse(remoteAddress, out remoteIPAddress))
                return false;

            if (parameters.TryGetValue(PARAM, out var remoteAddresses))
            {
                var addresses = remoteAddresses
                    .Split(',')
                    .Select(x => x.Trim())
                    .ToList();

                if (addresses.Contains(remoteAddress))
                    return true;

                var addressRanges = addresses
                    .Where(address => address.IndexOf('/') > -1)
                    .Select(address => new IPCIDRAddressRange(address))
                    .ToList();

                if (!addressRanges.Any()) 
                    return false;

                return addressRanges
                    .Any(range => range.Contains(remoteIPAddress));
            }

            return false;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}