using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Metrics;

namespace Unleash.Tests.Mock
{
    internal class MockApiClient : IUnleashApiClient
    {
        private static readonly ToggleCollection Toggles = new ToggleCollection(new List<FeatureToggle>
        {
            new FeatureToggle("one-enabled", true, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>(){
                    {"userIds", "userA" }
                })
            }),
            new FeatureToggle("one-disabled", false, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>()
                {
                    {"userIds", "userB" }
                })
            })
        });

        public Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FetchTogglesResult
            {
                HasChanged = true,
                Etag = "",
                ToggleCollection = Toggles
            });
        }

        public Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendMetrics(ThreadSafeMetricsBucket metricsBucket, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}