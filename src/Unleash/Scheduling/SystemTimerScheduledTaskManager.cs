using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Internal;

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

        public void Configure(IEnumerable<IUnleashScheduledTask> tasks, CancellationToken cancellationToken)
        {
            foreach (var task in tasks)
            {
                var name = task.Name;

                async void Callback(object state)
                {
                    if (!(state is CallbackState localState))
                        return;

                    if (!timers.TryGetValue(localState.Name, out Timer localTimer))
                        return;

                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        await task.ExecuteAsync(cancellationToken);
                    }
                    catch (TaskCanceledException taskCanceledException)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Logger.WarnException($"UNLEASH: Task '{name}' cancelled ...", taskCanceledException);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException($"UNLEASH: Unhandled exception from background task '{name}'.", ex);
                    }
                    finally
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // Do not schedule the next task
                        }
                        else
                        {
                            if (task.Interval == TimeSpanExecuteImmediately)
                            {
                                localTimer.SafeTimerChange(-1, Timeout.Infinite, ref disposeEnded);
                                Logger.Trace($"UNLEASH: Stopped background task '{name}'...");
                            }
                            else
                            {
                                localTimer.SafeTimerChange(task.Interval, Timeout.InfiniteTimeSpan, ref disposeEnded);
                                Logger.Trace($"UNLEASH: Scheduled background task '{name}' to run after '{task.Interval.TotalSeconds}' seconds...");
                            }
                        }
                    }
                }

                var dueTime = task.ExecuteDuringStartup
                    ? TimeSpanExecuteImmediately
                    : task.Interval;

                var callbackState = new CallbackState
                {
                    Name = name,
                    DueTime = dueTime
                };

                var timer = new Timer(
                    callback: Callback,
                    state: callbackState,
                    dueTime: dueTime,
                    period: Timeout.InfiniteTimeSpan);

                timers.Add(name, timer);
            }
        }

        private bool disposeEnded;
        public void Dispose()
        {
            if (disposeEnded)
                return;
            
            var timeout = TimeSpan.FromSeconds(1);

            using (var waitHandle = new ManualResetEvent(false))
            {
                foreach (var task in timers)
                {
                    // Returns false on second dispose
                    if (task.Value.Dispose(waitHandle))
                    {
                        if (!waitHandle.WaitOne(timeout))
                        {
                            throw new TimeoutException($"UNLEASH: Timeout waiting for task '{task.Key}' to stop..");
                        }
                    }
                }
            }

            disposeEnded = true;
            timers.Clear();
        }

        internal class CallbackState
        {
            public string Name { get; set; }
            public TimeSpan DueTime { get; set; }
        }
    }
}