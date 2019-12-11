using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sample.GenericHost
{
    public class TimerExecutorService<TTimerInvokedService> : BackgroundService
        where TTimerInvokedService : class, ITimerInvokedService
    {
        private IServiceScopeFactory ServiceScopeFactory { get; }
        private IOptionsMonitor<TimerInvokedServiceOptions> Options { get; }
        private ILogger Logger { get; }

        public TimerExecutorService(
            IServiceScopeFactory serviceScopeFactory,
            IOptionsMonitor<TimerInvokedServiceOptions> options,
            ILogger<TimerExecutorService<TTimerInvokedService>> logger)
        {
            ServiceScopeFactory = serviceScopeFactory;
            Options = options;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var intervalElapsedTime = await RunInterval(stoppingToken);
                await Delay(intervalElapsedTime, stoppingToken);
            }
        }

        private async Task<TimeSpan> RunInterval(CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.LogDebug("Starting interval...");

            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var scopeServiceProvider = scope.ServiceProvider;
                var executor = scopeServiceProvider.GetRequiredService<TTimerInvokedService>();
                await executor.ExecuteAsync(stoppingToken);
            }

            var intervalElapsedTime = stopwatch.Elapsed;
            Logger.LogDebug($"Interval completed in {intervalElapsedTime}.");

            return intervalElapsedTime;
        }

        private async Task Delay(TimeSpan intervalElapsedTime, CancellationToken stoppingToken)
        {
            var interval = Options.CurrentValue.Interval;
            var missedIntervalHandling = Options.CurrentValue.MissedIntervalHandling;
            var intervalRemaining = interval - intervalElapsedTime;

            if (intervalRemaining > TimeSpan.Zero)
            {
                Logger.LogDebug($"Sleeping for {intervalRemaining}...");
                await Task.Delay(intervalRemaining, stoppingToken);
            }
            else if (intervalRemaining < TimeSpan.Zero)
            {
                if (missedIntervalHandling == MissedIntervalHandling.WaitFullInterval)
                {
                    Logger.LogDebug($"Missed interval start time.  Sleeping until next interval in {intervalRemaining}...");
                    await Task.Delay(interval, stoppingToken);
                }
                else
                {
                    Logger.LogDebug($"Missed interval start time.  Running next interval immediately.");
                }
            }
        }
    }
}
