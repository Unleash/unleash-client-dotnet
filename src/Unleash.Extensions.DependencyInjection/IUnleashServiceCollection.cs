using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unleash
{
    public interface IUnleashServiceCollection : IServiceCollection
    {
        IConfiguration UnleashConfiguration { get; }
    }
}
