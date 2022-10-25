using System;
using System.Collections.Generic;
using System.Text;

namespace Unleash.Internal
{
    public class EventCallbackConfig
    {
        public Action<ImpressionEvent> ImpressionEvent { get; set; }
    }
}
