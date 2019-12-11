using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication.Admin.Dto;

namespace Unleash.Communication.Admin
{
    public interface IUnleashAdminStrategiesApiClient
    {
        Task<StrategiesResult> GetAllStrategies(CancellationToken cancellationToken = default(CancellationToken));
        Task CreateStrategy(Strategy strategy, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateStrategy(string strategyName, Strategy strategy, CancellationToken cancellationToken = default(CancellationToken));
    }
}