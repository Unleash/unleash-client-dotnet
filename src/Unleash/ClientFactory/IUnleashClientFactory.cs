using System.Threading.Tasks;
using Unleash.Strategies;

namespace Unleash.ClientFactory
{
    public interface IUnleashClientFactory
    {
        IUnleash CreateClient(UnleashSettings settings, bool synchronousInitialization = false, params IStrategy[] strategies);
        Task<IUnleash> CreateClientAsync(UnleashSettings settings, bool synchronousInitialization = false, params IStrategy[] strategies);
    }
}
