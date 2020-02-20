using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unleash.ClientFactory
{
    /// <inheritdoc />
    public class UnleashClientFactory : IUnleashClientFactory
    {
        ///// <summary>
        ///// Initializes a new instance of Unleash client. 
        ///// </summary>
        ///// <param name="SynchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        public async Task<IUnleash> Generate(UnleashSettings settings, bool SynchronousInitialization = false)
        {
            if (SynchronousInitialization)
            {
                settings.ScheduleFeatureToggleFetchImmediatly = false;
                var unleash = new DefaultUnleash(settings);
                await unleash.services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None);
                return unleash;
            }
            return new DefaultUnleash(settings);
        }
    }
}
