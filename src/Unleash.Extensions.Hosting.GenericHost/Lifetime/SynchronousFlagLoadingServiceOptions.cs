using System;

namespace Unleash.Lifetime
{
    public class SynchronousFlagLoadingServiceOptions
    {
        public bool OnlyOnEmptyCache { get; set; }
        public TimeSpan Timeout { get; set; }
    }
}
