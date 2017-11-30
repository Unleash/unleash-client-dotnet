using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Metrics;

namespace Unleash.Tests.Communication
{
    public class UnleashApiClient_RegisterClient_Tests : BaseUnleashApiClientTest
    {
        [Test]
        public async Task RegisterClient_Success()
        {
            var clientRegistration = new ClientRegistration()
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

            var result = await api.RegisterClient(clientRegistration, CancellationToken.None);
            result.Should().Be(true);
        }
    }
}