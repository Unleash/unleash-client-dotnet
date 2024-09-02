using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public interface IToggleBootstrapProvider
    {
        [Obsolete("Will return json string in the next major version", false)]
        ToggleCollection Read();
    }
}
