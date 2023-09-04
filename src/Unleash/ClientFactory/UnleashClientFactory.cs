using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Scheduling;
using Unleash.Strategies;

namespace Unleash.ClientFactory
{
    /// <inheritdoc />
    public class UnleashClientFactory : IUnleashClientFactory
    {
        private static readonly TaskFactory TaskFactory =
            new TaskFactory(CancellationToken.None,
                          TaskCreationOptions.None,
                          TaskContinuationOptions.None,
                          TaskScheduler.Default);

        /// <summary>
        /// Initializes a new instance of Unleash client. 
        /// </summary>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        /// <param name="strategies">Custom strategies, added in addtion to builtIn strategies.</param>
        public IUnleash CreateClient(UnleashSettings settings, bool synchronousInitialization = false, params IStrategy[] strategies)
        {
            if (synchronousInitialization)
            {
                try
                {

                    settings.ScheduleFeatureToggleFetchImmediatly = false;
                    var unleash = new DefaultUnleash(settings, strategies);
                    TaskFactory
                        .StartNew(() => unleash.services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None))
                        .Unwrap()
                        .GetAwaiter()
                        .GetResult();

                    return unleash;
                }
                catch (Exception ex)
                {
                    // Clean up
                    if (settings.ScheduledTaskManager?.GetType() == typeof(SystemTimerScheduledTaskManager))
                    {
                        settings.ScheduledTaskManager.Dispose();
                        settings.ScheduledTaskManager = new SystemTimerScheduledTaskManager();

                    }
                    throw new UnleashException("Unleash: Exception during synchronous initialization. See inner exception for details", ex);
                }
            }
            return new DefaultUnleash(settings, strategies);
        }


        /// <summary>
        /// Initializes a new instance of Unleash client. 
        /// </summary>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        /// <param name="strategies">Custom strategies, added in addtion to builtIn strategies.</param>
        public async Task<IUnleash> CreateClientAsync(UnleashSettings settings, bool synchronousInitialization = false, params IStrategy[] strategies)
        {
            if (synchronousInitialization)
            {
                try
                {
                    settings.ScheduleFeatureToggleFetchImmediatly = false;
                    var unleash = new DefaultUnleash(settings, strategies);
                    await unleash.services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                    return unleash;
                }
                catch (Exception ex)
                {
                    // Clean up
                    if (settings.ScheduledTaskManager?.GetType() == typeof(SystemTimerScheduledTaskManager))
                    {
                        settings.ScheduledTaskManager.Dispose();
                        settings.ScheduledTaskManager = new SystemTimerScheduledTaskManager();

                    }
                    throw new UnleashException("Unleash: Exception during synchronous initialization. See inner exception for details", ex);
                }
            }
            return new DefaultUnleash(settings, strategies);
        }
    }
}
