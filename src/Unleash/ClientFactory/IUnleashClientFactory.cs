using System;
using System.Threading.Tasks;
using Unleash.Strategies;

namespace Unleash.ClientFactory
{
    public interface IUnleashClientFactory
    {
        IUnleash CreateClient(UnleashSettings settings, bool synchronousInitialization = false, params Yggdrasil.IStrategy[] strategies);
        Task<IUnleash> CreateClientAsync(UnleashSettings settings, bool synchronousInitialization = false, params Yggdrasil.IStrategy[] strategies);
    }
}
