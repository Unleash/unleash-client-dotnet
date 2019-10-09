using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Metrics;

namespace Unleash.Tests.Metrics
{
    public class MetricsBucketTests
    {
        [Test]
        public void SingleBucket_No_WriteOperations()
        {
            var metricsBucket = new ThreadSafeMetricsBucket();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 50,
            };

            var totalTasks = 1000000;
            var numFeatureToggles = 50;

            Parallel.For(0, totalTasks, options, state =>
            {
                
                var i = (state % numFeatureToggles).ToString();
                var predicate = DateTime.Now.Ticks % 2 == 0;

                metricsBucket.RegisterCount(i, predicate);
            });

            using (metricsBucket.StopCollectingMetrics(out var bucket))
            {
                bucket.Toggles.Count.Should().Be(numFeatureToggles);

                // Should be cleared
                var count = GetNumberOfRegistrations(bucket);
                count.Should().Be(totalTasks);
            }
        }

        [Test]
        public void SingleBucket_OneCountPerTask3()
        {
            var metricsBucket = new ThreadSafeMetricsBucket();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10,
            };

            var totalTasks = 1000000L;
            var breaks = totalTasks / 10;
            long toggleCounter = 0;
            long recordedTasks = 0;

            var result = Parallel.For(1, totalTasks + 1, options, state =>
            {
                Interlocked.Increment(ref recordedTasks);

                var toggleName = (state % 50).ToString();
                var active = DateTime.Now.Ticks % 2 == 0;

                metricsBucket.RegisterCount(toggleName, active);

                if (state % breaks == 0)
                {
                    using (metricsBucket.StopCollectingMetrics(out var bucket))
                    {
                        var count = GetNumberOfRegistrations(bucket);

                        Interlocked.Add(ref toggleCounter, count);
                        Console.WriteLine($"Locking bucket at #{state} with {toggleCounter} toggle registrations");
                    }
                }
            });

            result.IsCompleted.Should().BeTrue();

            // Empty last bucket
            using (metricsBucket.StopCollectingMetrics(out var bucket))
            {
                var count = GetNumberOfRegistrations(bucket);
                Interlocked.Add(ref toggleCounter, count);
            }

            recordedTasks.Should().Be(totalTasks);

            var missedRegistrations = metricsBucket.MissedRegistrations;

            Console.WriteLine("# tasks: " + totalTasks);
            Console.WriteLine("# recorded tasks: " + recordedTasks);
            Console.WriteLine("# num entries: " + toggleCounter);
            Console.WriteLine("# num missed: " + missedRegistrations);
            Console.WriteLine();

            var total = toggleCounter + missedRegistrations;
            total.Should().Be(totalTasks);

            var percentageMissed = (double) missedRegistrations / totalTasks;
            percentageMissed.Should().BeLessThan(0.0001);//TODO: originally the value should be 0.00001, but appveyor are resource limited
        }

        private static long GetNumberOfRegistrations(MetricsBucket bucket)
        {
            return bucket.Toggles.Values
                .Select(x => x.Yes + x.No)
                .Sum();
        }
    }
}