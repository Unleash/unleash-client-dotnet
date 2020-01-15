using System.Collections.Concurrent;
using System.Threading;

namespace Unleash.Metrics
{
    internal class ToggleCount
    {
        private long yes;
        private long no;
        private ConcurrentDictionary<string, long> variants = new ConcurrentDictionary<string, long>();

        public long Yes => yes;
        public long No => no;
        public ConcurrentDictionary<string, long> Variants => variants;

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

        public void Register(string variantName)
        {
            var current = variants.GetOrAdd(variantName, s => new long());
            Interlocked.Increment(ref current);
        }

        /// <summary>
        /// Resets the counters to 0
        /// </summary>
        public void Reset()
        {
            yes = 0;
            no = 0;
            variants = new ConcurrentDictionary<string, long>();
        }
    }
}