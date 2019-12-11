using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Unleash.Tests.Integration
{
    public class ApiClientFetchTogglesTests : BaseUnleashApiClientIntegrationTests
    {
        [Fact]
        public async Task FetchToggles_WhenInvokedRepeatedly_ResultsInCacheHit()
        {
            var etag = string.Empty;
            var result1 = await Client.FetchToggles(etag, CancellationToken.None);

            Assert.True(result1.HasChanged);
            Assert.NotNull(result1.ToggleCollection);

            var result2 = await Client.FetchToggles(result1.Etag, CancellationToken.None);
            Assert.False(result2.HasChanged);
            Assert.Null(result2.ToggleCollection);
            Assert.Equal(result1.Etag, result2.Etag);
        }
    }
}
