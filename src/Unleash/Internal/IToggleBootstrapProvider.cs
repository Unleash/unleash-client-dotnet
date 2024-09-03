using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public interface IToggleBootstrapProvider
    {
        [Obsolete("Will be replaced in the next major version", false)]
        ToggleCollection Read();
    }
}
