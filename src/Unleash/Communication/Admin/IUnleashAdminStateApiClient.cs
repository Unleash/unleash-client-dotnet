using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin.Dto;

namespace Unleash.Communication.Admin
{
    public interface IUnleashAdminStateApiClient
    {
        Task<State> GetStateExport(bool includeFeatureToggles, bool includeStrategies, CancellationToken cancellationToken = default(CancellationToken));
        Task ImportState(State state, CancellationToken cancellationToken = default(CancellationToken));
    }
}
