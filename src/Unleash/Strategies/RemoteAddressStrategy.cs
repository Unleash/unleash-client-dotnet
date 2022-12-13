namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Unleash.Internal;
    using Unleash.Logging;
    using Unleash.Utilities;

    public class RemoteAddressStrategy : IStrategy
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultUnleash));
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

                var addressRanges = ToAddressRanges(addresses);

                if (!addressRanges.Any()) 
                    return false;

                return addressRanges
                    .Any(range => range.Contains(remoteIPAddress));
            }

            return false;
        }

        private List<IPCIDRAddressRange> ToAddressRanges(List<string> ipAddresses)
        {
            var addressRanges = new List<IPCIDRAddressRange>(ipAddresses.Count);
            foreach (var address in ipAddresses.Where(address => address.IndexOf('/') > -1))
            {
                try
                {
                    addressRanges.Add(new IPCIDRAddressRange(address));
                }
                catch (Exception ex)
                {
                    Logger.Error($"UNLEASH: RemoteAddressStrategy->ToAddressRanges threw exception: {ex.Message}. (Badly formatted IP/CIDR?)");
                }
            }

            return addressRanges;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}