using System;
using Microsoft.Extensions.DependencyInjection;

namespace Unleash
{
    using Communication;

    internal class HttpClientFactoryApiClientFactory : IUnleashApiClientFactory
    {
        private readonly IServiceProvider serviceProvider;

        public HttpClientFactoryApiClientFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IUnleashApiClient CreateClient() => serviceProvider.GetRequiredService<IUnleashApiClient>();
    }
}
