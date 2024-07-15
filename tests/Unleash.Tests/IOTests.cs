using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.ClientFactory;

namespace Unleash.Tests
{

    public class IOTests
    {
        private IUnleash unleash;

        private static void LockFile(object data)
        {
            var file = (string)data;
            using (var fs = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Task.Delay(10000).Wait();
            }
        }

        [Test]
        public async Task GracefullyFailsWhenFileLocked()
        {
            var settings = new MockedUnleashSettings(false, "test instance IOTests");
            
            var toggleFile = settings.GetFeatureToggleFilePath();
            var eTagFile = settings.GetFeatureToggleETagFilePath();

            Thread lockToggleFile = new Thread(LockFile);
            Thread lockETagFile = new Thread(LockFile);
            lockToggleFile.Start(toggleFile);
            lockETagFile.Start(eTagFile);

            var factory = new UnleashClientFactory();
            unleash = await factory.CreateClientAsync(settings, true);

            unleash.IsEnabled("one-enabled").Should().BeTrue();
        }
    }
}