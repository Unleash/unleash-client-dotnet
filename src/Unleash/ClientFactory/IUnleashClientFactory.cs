using System;
using System.Threading.Tasks;

namespace Unleash.ClientFactory
{
    public interface IUnleashClientFactory
    {
        Task<IUnleash> Generate(bool SynchronousInitialization);
    }
}
