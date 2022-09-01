namespace Unleash.Strategies
{
    using System.Collections.Generic;
    using System.Linq;
    using Unleash.Internal;

    public class RemoteAddressStrategy : IStrategy
    {
        internal static readonly string PARAM = "IPs";

        public string Name => "remoteAddress";

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context = null)
        {
            var remoteAddress = context?.RemoteAddress;

            if (string.IsNullOrEmpty(remoteAddress))
                return false;

            if (parameters.TryGetValue(PARAM, out var remoteAddresses))
            {
                return remoteAddresses
                    .Split(',')
                    .Select(x => x.Trim())
                    .Contains(remoteAddress);
            }

            return false;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}