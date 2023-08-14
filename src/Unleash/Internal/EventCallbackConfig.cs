using System;
using Unleash.Events;

namespace Unleash.Internal
{
    public class EventCallbackConfig
    {
        public Action<ImpressionEvent> ImpressionEvent { get; set; }
        public Action<ErrorEvent> ErrorEvent { get; set; }
        public Action<TogglesUpdatedEvent> TogglesUpdatedEvent { get; set; }

        public void RaiseError(ErrorEvent evt)
        {
            if (ErrorEvent != null)
            {
                ErrorEvent(evt);
            }
        }

        public void RaiseTogglesUpdated(TogglesUpdatedEvent evt)
        {
            if (TogglesUpdatedEvent != null)
            {
                TogglesUpdatedEvent(evt);
            }
        }

    }
}
