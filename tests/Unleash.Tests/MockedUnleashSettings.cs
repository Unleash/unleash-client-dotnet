using System;
using System.Collections.Generic;
using Unleash.Internal;
using Unleash.Tests.Mock;
using System.Text;

namespace Unleash.Tests
{
    public class MockedUnleashSettings : UnleashSettings
    {
        public MockedUnleashSettings(bool mockFileSystem = true)
        {
            AppName = "test";
            InstanceTag = "test instance 1";
            UnleashApi = new Uri("http://localhost:4242/");

            UnleashApiClient = new MockApiClient();
            FileSystem = new MockFileSystem();
            
            if (!mockFileSystem)
            {
                FileSystem = new FileSystem(Encoding.UTF8);
            }

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