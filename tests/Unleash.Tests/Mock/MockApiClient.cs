using Unleash.Communication;
using Unleash.Internal;
using Unleash.Metrics;
using Unleash.Variants;

namespace Unleash.Tests.Mock
{
    internal class MockApiClient : IUnleashApiClient
    {
        private static readonly string State = new List<FeatureToggle>
        {
            new FeatureToggle("one-enabled",  "release", true, false, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>(){
                    {"userIds", "userA" }
                })
            }, new List<VariantDefinition>()
            {
                new VariantDefinition("Aa", 33, null, null),
                new VariantDefinition("Aa", 33, null, null),
                new VariantDefinition("Ab", 34, null, new List<VariantOverride>{ new VariantOverride("context", new[] { "a", "b"}) }),
            }
            ),
            new FeatureToggle("one-disabled",  "release", false, false, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>()
                {
                    {"userIds", "userB" }
                })
            })
        }.ToString();

        public Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken, bool throwOnFail = false)
        {
            return Task.Run(async delegate
            {
                await Task.Delay(200);
                return new FetchTogglesResult
                {
                    HasChanged = true,
                    Etag = "etag",
                    State = State
                };
            });
        }

        public Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendMetrics(Yggdrasil.MetricsBucket metricsBucket, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}