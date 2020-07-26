using System.Threading;
using System.Threading.Tasks;

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
        public IUnleash CreateClient(UnleashSettings settings, bool synchronousInitialization = false)
        {
            if (synchronousInitialization)
            {
                settings.ScheduleFeatureToggleFetchImmediatly = false;
                var unleash = new DefaultUnleash(settings);
                TaskFactory
                    .StartNew(() => unleash.services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None))
                    .Unwrap()
                    .GetAwaiter()
                    .GetResult();
                
                return unleash;
            }
            return new DefaultUnleash(settings);
        }


        /// <summary>
        /// Initializes a new instance of Unleash client. 
        /// </summary>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        public async Task<IUnleash> CreateClientAsync(UnleashSettings settings, bool synchronousInitialization = false)
        {
            if (synchronousInitialization)
            {
                settings.ScheduleFeatureToggleFetchImmediatly = false;
                var unleash = new DefaultUnleash(settings);
                await unleash.services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                return unleash;
            }
            return new DefaultUnleash(settings);
        }
    }
}
