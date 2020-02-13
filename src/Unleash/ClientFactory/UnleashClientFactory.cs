using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unleash.ClientFactory
{
    /// <inheritdoc />
    public class UnleashClientFactory : IUnleashClientFactory
    {
        private UnleashSettings settings { get; }
        private CancellationTokenSource cancellationTokenSource { get; } = new CancellationTokenSource();

        ///// <summary>
        ///// Initializes a new instance of the client factory. 
        ///// </summary>
        ///// <param name="settings">Unleash settings</param>
        public UnleashClientFactory(UnleashSettings settings)
        {
            this.settings = settings;
            this.settings.ScheduleFeatureToggleFetchImmediatly = false;
        }

        ///// <summary>
        ///// Initializes a new instance of Unleash client. 
        ///// </summary>
        ///// <param name="SynchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        public async Task<IUnleash> Generate(bool SynchronousInitialization = false)
        {
            var unleash = new DefaultUnleash(settings);
            if (SynchronousInitialization)
            {
                await unleash.services.FetchFeatureTogglesTask.ExecuteAsync(cancellationTokenSource.Token);
            }

            return unleash;
        }
    }
}
