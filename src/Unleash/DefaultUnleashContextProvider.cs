using System.Collections.Generic;

namespace Unleash
{
    internal class DefaultUnleashContextProvider : IUnleashContextProvider
    {
        public DefaultUnleashContextProvider(UnleashContext context = null)
        {
            Context = context ?? new UnleashContext
            {
                Properties = new Dictionary<string, string>(0),
            };
        }

        public UnleashContext Context { get; }
    }
}