using System;
using System.Collections.Generic;

namespace Unleash
{
    internal class DefaultCustomHttpHeaderProvider : IUnleashCustomHttpHeaderProvider
    {
        public Dictionary<string, string> GetCustomHeaders()
        {
            return new Dictionary<string, string>();
        }
    }
}
