using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin.Dto;

namespace Unleash.Communication.Admin
{
    public interface IUnleashAdminFeatureTogglesApiClient
    {
        Task<FeatureToggleResult> GetAllActiveFeatureToggles(CancellationToken cancellationToken = default(CancellationToken));
        Task<FeatureToggle> GetFeatureToggle(string featureToggleName, CancellationToken cancellationToken = default(CancellationToken));
        Task CreateFeatureToggle(FeatureToggle featureToggle, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateFeatureToggle(string featureToggleName, FeatureToggle featureToggle, CancellationToken cancellationToken = default(CancellationToken));
        Task ArchiveFeatureToggle(string featureToggleName, CancellationToken cancellationToken = default(CancellationToken));
        Task EnableFeatureToggle(string featureToggleName, CancellationToken cancellationToken = default(CancellationToken));
        Task DisableFeatureToggle(string featureToggleName, CancellationToken cancellationToken = default(CancellationToken));
        Task<FeatureToggleResult> GetAllArchivedFeatureToggles(CancellationToken cancellationToken = default(CancellationToken));
        Task ReviveArchivedFeatureToggle(string featureToggleName, CancellationToken cancellationToken = default(CancellationToken));
    }
}