using Unleash.Serialization;

namespace Unleash.Communication
{
    internal class DefaultUnleashApiClientFactory : IUnleashApiClientFactory
    {
        private readonly UnleashSettings settings;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IJsonSerializer jsonSerializer;
        private readonly UnleashApiClientRequestHeaders unleashApiClientRequestHeaders;

        public DefaultUnleashApiClientFactory(UnleashSettings settings, IHttpClientFactory httpClientFactory,
            IJsonSerializer jsonSerializer, UnleashApiClientRequestHeaders unleashApiClientRequestHeaders)
        {
            this.settings = settings;
            this.httpClientFactory = httpClientFactory;
            this.jsonSerializer = jsonSerializer;
            this.unleashApiClientRequestHeaders = unleashApiClientRequestHeaders;
        }

        /// <inheritdoc />
        public IUnleashApiClient CreateClient()
        {
            return new UnleashApiClient(httpClientFactory.Create(settings.UnleashApi), jsonSerializer, unleashApiClientRequestHeaders);
        }
    }
}
