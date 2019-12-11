using System.Collections.Generic;

namespace Unleash.Tests.DotNetCore.Unleash
{
    public class ScopedDictionaryContextProvider : IUnleashContextProvider
    {
        // It might make sense for Properties to be a ConcurrentDictionary<string, string> in case an application were
        // to populate this from multiple threads/asynchronously.
        public UnleashContext Context { get; } = new UnleashContext
        {
            Properties = new Dictionary<string, string>()
        };
    }
}
