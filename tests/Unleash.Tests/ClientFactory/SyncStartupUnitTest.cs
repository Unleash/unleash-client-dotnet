using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unleash.Communication;
using Unleash.Metrics;
using FakeItEasy;
using Unleash.ClientFactory;
using Unleash.Serialization;
using System.Threading.Tasks;

namespace Unleash.Tests.StartupBehavior
{
    public class SyncStartupUnitTest
    {

        [Test]
        public async Task AsyncStartupClientStartup()
        {
            var mockApiClient = A.Fake<IUnleashApiClient>();
            var settings = new MockedUnleashSettings();
            settings.UnleashApiClient = mockApiClient;
            var unleashFactory = new UnleashClientFactory();

            IUnleash unleash = await unleashFactory.Generate(settings, SynchronousInitialization: true);

            A.CallTo(() => mockApiClient.FetchToggles(string.Empty, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
