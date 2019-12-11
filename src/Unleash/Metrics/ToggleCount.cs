using System.Threading;

namespace Unleash.Metrics
{
    public class ToggleCount
    {
        private long yes;
        private long no;

        public long Yes => yes;
        public long No => no;

        public void Register(bool active)
        {
            if (active)
            {
                Interlocked.Increment(ref yes);
            }
            else
            {
                Interlocked.Increment(ref no);
            }
        }

        /// <summary>
        /// Resets the counters to 0
        /// </summary>
        public void Reset()
        {
            yes = 0;
            no = 0;
        }
    }
}
