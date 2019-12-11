using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Metrics;
using Xunit;

namespace Unleash.Tests.Integration
{
    public class ApiClientRegisterClientTests : BaseUnleashApiClientIntegrationTests
    {
        [Fact]
        public async Task RegisterClient_WhenInvoked_CompletesSuccessfully()
        {
            var clientRegistration = new ClientRegistration
            {
                AppName = GetType().Name,
                InstanceId = "instance1",
                Interval = 1000,
                SdkVersion = "sdk101",
                Started = DateTimeOffset.UtcNow,
                Strategies = new List<string>
                {
                    "abc"
                }
            };

            var result = await Client.RegisterClient(clientRegistration, CancellationToken.None);

            Assert.True(result);
        }
    }
}
