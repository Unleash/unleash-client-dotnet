namespace Unleash.Communication
{
    internal interface IUnleashApiClientFactory
    {
        IUnleashApiClient CreateClient();
    }
}
