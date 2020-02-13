using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unleash.Communication;
using FakeItEasy;
using System.Threading.Tasks;
using Unleash.ClientFactory;

namespace Unleash.Tests.ClientFactory
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
