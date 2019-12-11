using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Unleash.Tests.AspNetCore.Common;
using Unleash.Tests.DotNetCore.AspNetCore.Testing;
using Xunit;

namespace Unleash.Tests.AspNetCore
{
    public class UnleashAspNetCoreTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private WebApplicationFactory<Startup> Factory { get; }

        public UnleashAspNetCoreTests(TestWebApplicationFactory<Startup> factory)
        {
            Factory = factory;
        }

        [Fact]
        public async Task TestControllerTestRequest_ShouldExecute_ButDiesWithDIIssue()
        {
            var client = Factory.CreateClient();
            var response = await client.GetAsync("FlagTest").ConfigureAwait(false);

            // I don't really have anything to test here, but this test won't/can't pass anyway b/c of DI issues.
            // The controller can't be constructed
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task TestControllerPropertiesEndpoint_ShouldReturnMiddlewareAndActionFilterAttributes()
        {
            var client = Factory.CreateClient();
            var response = await client.GetAsync("ContextProviderKeys").ConfigureAwait(false);

            // This test will also fail due to DI issues... but if it passed, I would expect to see the 2 keys from the
            // middleware and the MVC ActionFilter key.  They'd be writing to the same Dictionary<string, string> in the
            // ScopedDictionaryContextProvider b/c that provider was registered using AddScoped<> and ASP.NET Core
            // creates a scope at the beginning of the HTTP request (very early in the request processing).

            // The controller can't be constructed
            response.EnsureSuccessStatusCode();

            var dict = await response.Content.ReadAsAsync<Dictionary<string, string>>().ConfigureAwait(false);

            Assert.True(dict.ContainsKey("Method"));
            Assert.True(dict.ContainsKey("Path"));
            Assert.True(dict.ContainsKey("ActionDisplayName"));
        }
    }
}
