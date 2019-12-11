using System;

namespace Sample.GenericHost
{
    public class TimerInvokedServiceOptions
    {
        public TimeSpan Interval { get; set; }
        public MissedIntervalHandling MissedIntervalHandling { get; set; }
    }
}
