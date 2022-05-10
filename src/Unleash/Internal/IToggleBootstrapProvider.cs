using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public interface IToggleBootstrapProvider
    {
        ToggleCollection Read();
    }
}
