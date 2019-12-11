using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin.Dto;
using Unleash.Serialization;

namespace Unleash.Communication.Admin
{
    public class UnleashAdminApiClient : BaseAdminApiClient, IUnleashAdminApiClient
    {
        public IUnleashAdminFeatureTogglesApiClient FeatureToggles { get; }
        public IUnleashAdminStrategiesApiClient Strategies { get; }
        public IUnleashAdminMetricsApiClient Metrics { get; }
        public IUnleashAdminEventsApiClient Events { get; }
        public IUnleashAdminStateApiClient State { get; }

        public UnleashAdminApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer)
            : base(httpClient, jsonSerializer)
        {
            FeatureToggles = new UnleashAdminFeatureTogglesApiClient(httpClient, jsonSerializer);
            Strategies = new UnleashAdminStrategiesApiClient(httpClient, jsonSerializer);
            Metrics = new UnleashAdminMetricsApiClient(httpClient, jsonSerializer);
            Events = new UnleashAdminEventsApiClient(httpClient, jsonSerializer);
            State = new UnleashAdminStateApiClient(httpClient, jsonSerializer);
        }

        public Task Authenticate(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            return PostAsync("api/admin/login", new Login {Email = emailAddress}, cancellationToken);
        }

        private class UnleashAdminFeatureTogglesApiClient : BaseAdminApiClient, IUnleashAdminFeatureTogglesApiClient
        {
            public UnleashAdminFeatureTogglesApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer) : base(
                httpClient, jsonSerializer)
            {
            }

            public Task<FeatureToggleResult> GetAllActiveFeatureToggles(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<FeatureToggleResult>("api/admin/features", cancellationToken);
            }

            public Task<FeatureToggle> GetFeatureToggle(string featureToggleName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<FeatureToggle>($"api/admin/features/{featureToggleName}", cancellationToken);
            }

            public Task CreateFeatureToggle(FeatureToggle featureToggle,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return PostAsync("api/admin/features/", featureToggle, cancellationToken);
            }

            public Task UpdateFeatureToggle(string featureToggleName, FeatureToggle featureToggle,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return PutAsync($"api/admin/features/{featureToggleName}", featureToggle, cancellationToken);
            }

            public Task ArchiveFeatureToggle(string featureToggleName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return DeleteAsync($"api/admin/features/{featureToggleName}", cancellationToken);
            }

            public Task EnableFeatureToggle(string featureToggleName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return PostAsync($"api/admin/features/{featureToggleName}/toggle/on", cancellationToken);
            }

            public Task DisableFeatureToggle(string featureToggleName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return PostAsync($"api/admin/features/{featureToggleName}/toggle/off", cancellationToken);
            }

            public Task<FeatureToggleResult> GetAllArchivedFeatureToggles(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<FeatureToggleResult>("api/admin/archive/features", cancellationToken);
            }

            public Task ReviveArchivedFeatureToggle(string featureToggleName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                var reviveFeature = new ReviveFeature {Name = featureToggleName};
                return PostAsync("api/admin/archive/revive", reviveFeature, cancellationToken);
            }
        }

        private class UnleashAdminStrategiesApiClient : BaseAdminApiClient, IUnleashAdminStrategiesApiClient
        {
            public UnleashAdminStrategiesApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer) : base(
                httpClient, jsonSerializer)
            {
            }

            public Task<StrategiesResult> GetAllStrategies(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<StrategiesResult>("api/admin/strategies", cancellationToken);
            }

            public Task CreateStrategy(Strategy strategy,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return PostAsync("api/admin/strategies", strategy, cancellationToken);
            }

            public Task UpdateStrategy(string strategyName, Strategy strategy,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return PutAsync($"api/admin/strategies/{strategyName}", strategy, cancellationToken);
            }
        }

        private class UnleashAdminMetricsApiClient : BaseAdminApiClient, IUnleashAdminMetricsApiClient
        {
            public UnleashAdminMetricsApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer) : base(
                httpClient, jsonSerializer)
            {
            }

            public Task<SeenTogglesMetricsEntry[]> GetSeenToggles(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<SeenTogglesMetricsEntry[]>("api/admin/metrics/seen-toggles", cancellationToken);
            }

            public Task<FeatureTogglesMetrics> GetFeatureTogglesMetrics(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<FeatureTogglesMetrics>("api/admin/metrics/feature-toggles", cancellationToken);
            }

            public Task<ApplicationsResult> GetApplications(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<ApplicationsResult>("api/admin/metrics/applications", cancellationToken);
            }

            public Task<ApplicationsResult> GetApplicationsImplementingStrategy(string strategyName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                var query = new NameValueCollection
                {
                    ["strategyName"] = strategyName
                };

                return GetAsync<ApplicationsResult>("api/admin/metrics/applications", query, cancellationToken);
            }

            public Task<ApplicationDetail> GetApplicationDetails(string applicationName,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<ApplicationDetail>($"api/admin/metrics/applications/{applicationName}", cancellationToken);
            }

            public Task<SeenApplications> GetSeenApplications(
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<SeenApplications>("api/admin/metrics/seen-apps", cancellationToken);
            }
        }

        private class UnleashAdminEventsApiClient : BaseAdminApiClient, IUnleashAdminEventsApiClient
        {
            public UnleashAdminEventsApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer) : base(httpClient,
                jsonSerializer)
            {
            }

            public Task<EventsResult> GetEvents(CancellationToken cancellationToken = default(CancellationToken))
            {
                return GetAsync<EventsResult>("api/admin/events", cancellationToken);
            }
        }

        private class UnleashAdminStateApiClient : BaseAdminApiClient, IUnleashAdminStateApiClient
        {
            public UnleashAdminStateApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer) : base(httpClient,
                jsonSerializer)
            {
            }

            public Task<State> GetStateExport(bool includeFeatureToggles, bool includeStrategies,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                var query = new NameValueCollection
                {
                    ["format"] = "json",
                    ["featureToggles"] = includeFeatureToggles ? "1" : "0",
                    ["strategies"] = includeStrategies ? "1" : "0"
                };

                return GetAsync<State>("api/admin/state/export", query, cancellationToken);
            }

            public Task ImportState(State state, CancellationToken cancellationToken = default(CancellationToken))
            {
                return PostAsync("api/admin/state/import", state, cancellationToken);
            }
        }
    }
}
