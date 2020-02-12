using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unleash.ClientFactory
{
    public class UnleashClientFactory : IUnleashClientFactory
    {
        private UnleashSettings settings { get; }
        private CancellationTokenSource cancellationTokenSource { get; } = new CancellationTokenSource();


        public UnleashClientFactory(UnleashSettings settings)
        {
            this.settings = settings;
        }


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
