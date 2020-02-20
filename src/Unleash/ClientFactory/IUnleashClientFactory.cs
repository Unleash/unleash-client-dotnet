using System;
using System.Threading.Tasks;

namespace Unleash.ClientFactory
{
    public interface IUnleashClientFactory
    {
        Task<IUnleash> Generate(UnleashSettings settings, bool SynchronousInitialization = false);
    }
}
