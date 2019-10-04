using System.Collections.Generic;

namespace Unleash
{
    internal class DefaultUnleashContextProvider : IUnleashContextProvider
    {
        public DefaultUnleashContextProvider(UnleashContext context = null)
        {
            Context = context ?? new UnleashContext
            {
                UserId = string.Empty,
                SessionId = string.Empty,
                RemoteAddress = string.Empty,
                Properties = new Dictionary<string, string>(0),
                Environment = "default",
                AppName = "my-app"
            };
        }

        public UnleashContext Context { get; }
    }
}