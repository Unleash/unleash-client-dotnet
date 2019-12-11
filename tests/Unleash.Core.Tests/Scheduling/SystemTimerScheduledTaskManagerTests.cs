using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Scheduling;
using Xunit;
using Xunit.Abstractions;

namespace Unleash.Core.Tests.Scheduling
{
    public class SystemTimerScheduledTaskManagerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SystemTimerScheduledTaskManagerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private class TestBackgroundTask : IUnleashScheduledTask
        {
            public string Name => "test-task";
            public TimeSpan Interval { get; set; }
            public bool ExecuteDuringStartup { get; set; } = false;
            public Task Completion => _completionSource.Task;
            public TimeSpan ExecutionDelay {get; set;}
            public int Counter { get; private set; }
            public int? ThrowAfter { get; set; }

            private int StopAfter { get; }

            private readonly TaskCompletionSource<object> _completionSource;

            public TestBackgroundTask(int stopAfter, int? throwAfter = null, TimeSpan? executionDelay = null, TimeSpan? interval = null, CancellationToken cancellationToken = default)
            {
                StopAfter = stopAfter;
                ThrowAfter = throwAfter;
                ExecutionDelay = executionDelay ?? TimeSpan.FromMilliseconds(10);
                Interval = interval ?? TimeSpan.FromMilliseconds(20);

                _completionSource = new TaskCompletionSource<object>();

                cancellationToken.Register(() =>
                {
                    _completionSource.SetCanceled();
                });
            }

            public async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _completionSource.SetCanceled();
                }

                Counter++;

                try
                {
                    await Task.Delay(ExecutionDelay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Gulp
                }

                if (Counter == StopAfter)
                {
                    _completionSource.SetResult(null);
                }

                if (ThrowAfter.HasValue && Counter == ThrowAfter.Value)
                {
                    throw new Exception($"Failing task {Counter}");
                }
            }
        }

        [Fact]
        public async Task Scheduler_WhenTaskExceptionOccurs_ShouldContinueToRun()
        {
            using (var scheduler = new SystemTimerScheduledTaskManager())
            {
                var task = new TestBackgroundTask(5, 1);

                scheduler.Configure(new[] { task }, CancellationToken.None);

                try
                {
                    await task.Completion;
                }
                catch (OperationCanceledException)
                {
                    Assert.False(true);
                }

                Assert.Equal(5, task.Counter);
            }
        }

        [Fact]
        public async Task Scheduler_WhenCancellationTokenIsCancelled_ShutsDownAsExpected()
        {
            using (var cts = new CancellationTokenSource())
            using (var scheduler = new SystemTimerScheduledTaskManager())
            {
                var task = new TestBackgroundTask(
                    5,
                    executionDelay: TimeSpan.Zero,
                    interval: TimeSpan.FromMilliseconds(200),
                    cancellationToken: cts.Token);

                cts.CancelAfter(500);

                scheduler.Configure(new[] { task }, cts.Token);

                try
                {
                    await task.Completion;
                    Assert.False(true);
                }
                catch (OperationCanceledException)
                {
                    _testOutputHelper.WriteLine("Cancelled: " + task.Counter);
                    Assert.InRange(task.Counter, 2, 3);
                }
            }

            Assert.True(true);
        }
    }
}
