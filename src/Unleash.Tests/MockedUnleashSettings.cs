using System;
using System.Collections.Generic;
using Unleash.Tests.Mock;

namespace Unleash.Tests
{
    public class MockedUnleashSettings : UnleashSettings
    {
        public MockedUnleashSettings()
        {
            AppName = "test";
            InstanceTag = "test instance 1";
            UnleashApi = new Uri("http://localhost:4242/");

            UnleashApiClient = new MockApiClient();
            FileSystem = new MockFileSystem();

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