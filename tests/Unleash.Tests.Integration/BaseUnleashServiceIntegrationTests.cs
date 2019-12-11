using Unleash.Internal;
using Unleash.Tests.Integration.Fixtures;
using Xunit;

namespace Unleash.Tests.Integration
{
    [Collection("Unleash")]
    public abstract class BaseUnleashServiceIntegrationTests
    {
        protected UnleashSettings Settings => UnleashServiceFixture.Settings;
        protected IUnleash Unleash => UnleashServiceFixture.Unleash;
        protected IUnleashServices UnleashServices => UnleashServiceFixture.UnleashServices;
        protected IUnleashContextProvider ContextProvider => UnleashServiceFixture.ContextProvider;

        private UnleashServiceFixture UnleashServiceFixture { get; }

        protected BaseUnleashServiceIntegrationTests(UnleashServiceFixture unleashServiceFixture)
        {
            UnleashServiceFixture = unleashServiceFixture;
        }
    }
}
