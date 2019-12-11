using Xunit;

namespace Unleash.Tests.Integration.Fixtures
{
    [CollectionDefinition("Unleash")]
    public class UnleashServiceCollection : ICollectionFixture<UnleashServiceFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
