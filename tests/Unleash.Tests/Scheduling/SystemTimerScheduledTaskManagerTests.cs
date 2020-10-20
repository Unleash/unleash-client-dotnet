using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Scheduling;

namespace Unleash.Tests.Scheduling
{
    public class SystemTimerScheduledTaskManagerTests
    {
        class TestBackgroundTask : IUnleashScheduledTask
        {
            public ManualResetEventSlim Reset { get; set; }
            public TimeSpan ExecutionDelay {get;set;}
            public int Counter { get; set; } = 1;
            public bool ThrowsExceptions { get; set; }

            public async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                Counter++;

                await Task.Delay(ExecutionDelay, cancellationToken);

                if (Counter == 5)
                    Reset?.Set();

                if (ThrowsExceptions)
                    throw new Exception("Wops, failing task... " + Counter);
            }

            public string Name => "test-task";
            public TimeSpan Interval { get; set; }
            public bool ExecuteDuringStartup { get; set; }
        }

        [Test]
        public void ShouldContinueToRunEvenThoughExceptionsOccurs()
        {
            using (var reset = new ManualResetEventSlim(false))
            using (var scheduler = new SystemTimerScheduledTaskManager())
            {
                var task = new TestBackgroundTask()
                {
                    ExecutionDelay = TimeSpan.FromMilliseconds(10),
                    ExecuteDuringStartup = false,
                    Reset = reset,
                    Interval = TimeSpan.FromMilliseconds(20),
                    ThrowsExceptions = true
                };

                scheduler.Configure(new List<IUnleashScheduledTask>() { task }, CancellationToken.None);
                reset.Wait();

                task.Counter.Should().Be(5);
            }
        }

        [Test]
        public void ShouldBeStoppedWhenCancelled()
        {
            using (var reset = new ManualResetEventSlim(false))
            using (var cts = new CancellationTokenSource())
            using (var scheduler = new SystemTimerScheduledTaskManager())
            {
                var task = new TestBackgroundTask()
                {
                    ExecutionDelay = TimeSpan.FromMilliseconds(10),
                    Reset = reset,
                    Interval = TimeSpan.FromMilliseconds(10),
                };

                scheduler.Configure(new List<IUnleashScheduledTask>() { task }, cts.Token);

                cts.CancelAfter(75);

                reset.Wait(TimeSpan.FromMilliseconds(200));
                
                task.Counter.Should().BeInRange(4, 6);
            }
        }
    }
}