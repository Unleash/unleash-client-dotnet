using System;
using System.Collections.Generic;
using System.Threading;
using Unleash.Logging;

namespace Unleash.Scheduling
{
    /// <inheritdoc />
    /// <summary>
    /// Default task manager based on System.Threading.Timers.
    /// </summary>
    internal class SystemTimerScheduledTaskManager : IUnleashScheduledTaskManager
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(SystemTimerScheduledTaskManager));
        private static readonly TimeSpan TimeSpanExecuteImmediately = TimeSpan.Zero;

        private readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        private long runningTasks = 0;

        private readonly object syncObject = new object();
        private bool disposed = false;

        public void Configure(IEnumerable<IUnleashScheduledTask> tasks, CancellationToken cancellationToken)
        {
            foreach (var task in tasks)
            {
                async void Callback(object state)
                {
                    if (!(state is string taskName) || !timers.TryGetValue(taskName, out Timer localTimer))
                    {
                        return;
                    }

                    try
                    {
                        Interlocked.Increment(ref runningTasks);

                        cancellationToken.ThrowIfCancellationRequested();
                        await task.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException operationCanceledException)
                    {
                        Logger.ErrorException($"UNLEASH: Task '{task.Name}' cancelled ...", operationCanceledException);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException($"UNLEASH: Unhandled exception from background task '{task.Name}'.", ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref runningTasks);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            this.ResetTimer(localTimer, task.Name, task.Interval);
                        }
                    }
                }

                var dueTime = task.ExecuteDuringStartup
                    ? TimeSpanExecuteImmediately
                    : task.Interval;

                var timer = new Timer(
                    callback: Callback,
                    state: task.Name,
                    dueTime: dueTime,
                    period: Timeout.InfiniteTimeSpan);

                timers.Add(task.Name, timer);
            }
        }

        private void ResetTimer(Timer localTimer, string taskName, TimeSpan taskInterval)
        {
            if (!this.disposed)
            {
                lock (this.syncObject)
                {
                    if (!this.disposed)
                    {
                        if (taskInterval == TimeSpanExecuteImmediately)
                        {
                            localTimer.Change(-1, Timeout.Infinite);
                            Logger.Trace($"UNLEASH: Stopped background task '{taskName}'...");
                        }
                        else
                        {
                            localTimer.Change(taskInterval, Timeout.InfiniteTimeSpan);
                            Logger.Trace(
                                $"UNLEASH: Scheduled background task '{taskName}' to run after '{taskInterval.TotalSeconds}' seconds...");
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                lock (syncObject)
                {
                    if (!disposed)
                    {
                        foreach (var task in timers)
                        {
                            task.Value.Dispose();
                        }

                        timers.Clear();

                        var now = DateTime.Now;
                        while (true)
                        {
                            var currentlyRunningTasks = Interlocked.Read(ref runningTasks);
                            if (currentlyRunningTasks == 0)
                            {
                                break;
                            }

                            if (DateTime.Now - now > TimeSpan.FromSeconds(1))
                            {
                                throw new TimeoutException($"UNLEASH: Timeout waiting for tasks to stop..");
                            }

                            Thread.Sleep(200);
                        }

                        disposed = true;
                    }
                }
            }
        }
    }
}
