using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin.Dto;

namespace Unleash.Communication.Admin
{
    public interface IUnleashAdminMetricsApiClient
    {
        Task<SeenTogglesMetricsEntry[]> GetSeenToggles(CancellationToken cancellationToken = default(CancellationToken));
        Task<FeatureTogglesMetrics> GetFeatureTogglesMetrics(CancellationToken cancellationToken = default(CancellationToken));
        Task<ApplicationsResult> GetApplications(CancellationToken cancellationToken = default(CancellationToken));
        Task<ApplicationsResult> GetApplicationsImplementingStrategy(string strategyName, CancellationToken cancellationToken = default(CancellationToken));
        Task<ApplicationDetail> GetApplicationDetails(string applicationName, CancellationToken cancellationToken = default(CancellationToken));
        Task<SeenApplications> GetSeenApplications(CancellationToken cancellationToken = default(CancellationToken));
    }
}
