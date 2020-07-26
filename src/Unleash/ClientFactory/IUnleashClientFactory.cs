using System;
using System.Threading.Tasks;

namespace Unleash.ClientFactory
{
    public interface IUnleashClientFactory
    {
        IUnleash CreateClient(UnleashSettings settings, bool synchronousInitialization = false);
        Task<IUnleash> CreateClientAsync(UnleashSettings settings, bool synchronousInitialization = false);
    }
}
