using System;
using System.Net.Http.Headers;
using System.Threading;

namespace Unleash.Internal
{
    internal static class UnleashExtensions
    {
        internal static void AddContentTypeJson(this HttpContentHeaders headers)
        {
            const string contentType = "Content-Type";
            const string applicationJson = "application/json";

            headers.TryAddWithoutValidation(contentType, applicationJson);
        }

        internal static void SafeTimerChange(this Timer timer, dynamic dueTime, dynamic period, ref bool disposeEnded)
        {
            if (dueTime.GetType() != period.GetType())
                throw new Exception("Data types has to match. (Int32 or TimeSpan)");

            if (!(dueTime.GetType() != typeof(int) || dueTime.GetType() != typeof(TimeSpan)))
                throw new Exception("Only System.Int32 or System.TimeSpan");

            try
            {
                timer?.Change(dueTime, period);
            }
            catch (ObjectDisposedException)
            {
                // race condition with Dispose can cause trigger to be called when underlying
                // timer is being disposed - and a change will fail in this case.
                // see
                // https://msdn.microsoft.com/en-us/library/b97tkt95(v=vs.110).aspx#Anchor_2
                if (disposeEnded)
                {
                    // we still want to throw the exception in case someone really tries
                    // to change the timer after disposal has finished
                    // of course there's a slight race condition here where we might not
                    // throw even though disposal is already done.
                    // since the offending code would most likely already be "failing"
                    // unreliably i personally can live with increasing the
                    // "unreliable failure" time-window slightly
                    throw;
                }
            }
        }
    }
}