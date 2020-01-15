using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Unleash.Metrics
{
    /// <inheritdoc />
    /// <summary>
    /// Provides synchronization that supports multiple registration counters and single 'writer' (transfer to server)
    /// 
    /// While in write mode, no registrations will occur. i.e: no lock for rest of system.
    /// </summary>
    internal class ThreadSafeMetricsBucket : IDisposable
    {
        private long missedRegistrations;
        public long MissedRegistrations => missedRegistrations;

        private readonly MetricsBucket metricsBucket;

        private readonly ReaderWriterLockSlim @lock = 
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public ThreadSafeMetricsBucket(MetricsBucket metricsBucket = null)
        {
            this.metricsBucket = metricsBucket ?? new MetricsBucket();

            this.metricsBucket.Toggles = new ConcurrentDictionary<string, ToggleCount>();
            this.metricsBucket.Start = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Registers a new toggle count given a read-lock can be aquired.
        /// </summary>
        /// <param name="toggleName">The name of the toggle.</param>
        /// <param name="active">True or False</param>
        public void RegisterCount(string toggleName, bool active)
        {
            WithToggleCount(toggleName, toggle => toggle.Register(active));
        }

        public void RegisterCount(string toggleName, string variantName)
        {
            WithToggleCount(toggleName, toggle => toggle.Register(variantName));
        }

        private void WithToggleCount(string toggleName, Action<ToggleCount> action)
        {
            if (@lock.TryEnterReadLock(2))
            {
                try
                {
                    var toggle = metricsBucket.Toggles.GetOrAdd(toggleName, x => new ToggleCount());
                    action(toggle);

                }
                finally
                {
                    @lock.ExitReadLock();
                }
            }
            else
            {
                // Ignore
                Interlocked.Increment(ref missedRegistrations);
            }
        }

        /// <summary>
        /// Use withing using-statement. New registrations will not be added.
        /// </summary>
        public IDisposable StopCollectingMetrics(out MetricsBucket bucket)
        {
            @lock.EnterWriteLock();
            
            bucket = metricsBucket;
            bucket.Stop = DateTimeOffset.UtcNow;

            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Resets the counters to 0.
        /// </summary>
        void IDisposable.Dispose()
        {
            ResetCounters();
            @lock.ExitWriteLock();
        }

        private void ResetCounters()
        {
            metricsBucket.Start = DateTimeOffset.UtcNow;

            foreach (var item in metricsBucket.Toggles)
                item.Value.Reset();
        }
    }

    internal class MetricsBucket
    {
        public ConcurrentDictionary<string, ToggleCount> Toggles { get; set; }

        public DateTimeOffset Start { get; set; }
        public DateTimeOffset Stop { get; set; }
    }
}