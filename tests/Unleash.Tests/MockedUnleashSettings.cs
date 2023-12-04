using System;
using System.Collections.Generic;
using Unleash.Internal;
using Unleash.Tests.Mock;
using System.Text;

namespace Unleash.Tests
{
    public class MockedUnleashSettings : UnleashSettings
    {
        public MockedUnleashSettings()
        {
            AppName = "test";
            UnleashApi = new Uri("http://localhost:4242/");

            UnleashApiClient = new MockApiClient();
            
            FileSystem = new FileSystem(Encoding.UTF8);

            UnleashContextProvider = new DefaultUnleashContextProvider(new UnleashContext
            {
                UserId = "userA",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            });
        }
    }
}