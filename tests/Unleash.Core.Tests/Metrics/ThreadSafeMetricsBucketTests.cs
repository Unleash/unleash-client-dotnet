using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Metrics;
using Xunit;
using Xunit.Abstractions;

namespace Unleash.Core.Tests.Metrics
{
    public class ThreadSafeMetricsBucketTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThreadSafeMetricsBucketTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(50, 10000, 50)]
        public void RegisterCount_WhenInvokedFromMultipleThreadsAndCheckedAtCompletion_BehavesPredictably(int maxDegreeOfParallelism, int totalRegistrations, int featureToggleCount)
        {
            var metricsBucket = new ThreadSafeMetricsBucket();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var exceptions = new ConcurrentBag<Exception>();
            var result = Parallel.For(0, totalRegistrations, options, state =>
            {
                try
                {
                    var toggleName = (state % featureToggleCount).ToString();
                    var active = DateTime.Now.Ticks % 2 == 0;

                    metricsBucket.RegisterCount(toggleName, active);
                }
                catch (Exception exc)
                {
                    exceptions.Add(exc);
                }
            });

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            Assert.True(result.IsCompleted);

            using (metricsBucket.StopCollectingMetrics(out var bucket))
            {
                Assert.Equal(featureToggleCount, bucket.Toggles.Count);

                var actualRegistrations = GetNumberOfRegistrations(bucket);
                Assert.Equal(totalRegistrations, actualRegistrations);
            }
        }

        [Theory]
        [InlineData(10, 1000000L, 50, 0.1)]
        public void RegisterCount_WhenInvokedFromMultipleThreadsAndCheckedIncrementally_BehavesPredictably(int maxDegreeOfParallelism, long totalRegistrations, int featureToggleCount, double breakPercent)
        {
            var metricsBucket = new ThreadSafeMetricsBucket();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var breaks = totalRegistrations * breakPercent;

            var toggleCounter = 0L;
            var recordedTasks = 0L;
            var exceptions = new ConcurrentBag<Exception>();
            var result = Parallel.For(1, totalRegistrations + 1, options, state =>
            {
                try
                {
                    Interlocked.Increment(ref recordedTasks);

                    var toggleName = (state % featureToggleCount).ToString();
                    var active = DateTime.Now.Ticks % 2 == 0;

                    metricsBucket.RegisterCount(toggleName, active);

                    if (state % breaks == 0)
                    {
                        using (metricsBucket.StopCollectingMetrics(out var bucket))
                        {
                            var count = GetNumberOfRegistrations(bucket);

                            Interlocked.Add(ref toggleCounter, count);
                            _testOutputHelper.WriteLine($"Locking bucket at #{state} with {toggleCounter} toggle registrations");
                        }
                    }
                }
                catch (Exception exc)
                {
                    exceptions.Add(exc);
                }
            });

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            Assert.True(result.IsCompleted);

            // Empty last bucket
            using (metricsBucket.StopCollectingMetrics(out var bucket))
            {
                var count = GetNumberOfRegistrations(bucket);
                Interlocked.Add(ref toggleCounter, count);
            }

            Assert.Equal(totalRegistrations, recordedTasks);

            var missedRegistrations = metricsBucket.MissedRegistrations;
            var percentageMissed = (double) missedRegistrations / totalRegistrations;

            _testOutputHelper.WriteLine("# tasks: {0}", totalRegistrations);
            _testOutputHelper.WriteLine("# recorded tasks: {0}", recordedTasks);
            _testOutputHelper.WriteLine("# num entries: {0}", toggleCounter);
            _testOutputHelper.WriteLine("# num missed: {0}", missedRegistrations);
            _testOutputHelper.WriteLine("# pct missed: {0:N5}", percentageMissed);

            var total = toggleCounter + missedRegistrations;
            Assert.Equal(total, totalRegistrations);

            Assert.True(percentageMissed < 0.00025);
        }

        private static long GetNumberOfRegistrations(MetricsBucket bucket)
        {
            return bucket.Toggles.Values
                .Select(x => x.Yes + x.No)
                .Sum();
        }
    }
}
