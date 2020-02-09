using System.Collections.Generic;

namespace Unleash
{
    public interface IUnleashCustomHttpHeaderProvider
    {
       Dictionary<string, string> CustomHeaders { get; }
    }
}
