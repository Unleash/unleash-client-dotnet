using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Unleash.Repository;

namespace Unleash.Tests
{
    public class UnleashTestConfig : UnleashConfig
    {
        private static readonly ToggleCollection Toggles = new ToggleCollection(new List<FeatureToggle>
        {
            new FeatureToggle("one-enabled", true, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>(){
                    {"userIds", "userA" }
                })
            }),
            new FeatureToggle("one-disabled", false, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>()
                {
                    {"userIds", "userB" }
                })
            })
        });

        public UnleashTestConfig()
        {
            SetAppName("test");
            SetInstanceId("test instance 1");
            SetUnleashApi("http://localhost:4242/");
            SetBackgroundTasksDisabled();
            SetDataSource(Toggles);
            ContextProvider = new DefaultUnleashContextProvider(
                new UnleashContext(
                    "userA", 
                    "sessionId", 
                    "remoteAddress", 
                    new Dictionary<string, string>()));
        }

        public UnleashTestConfig SetDataSource(string appDataToggleCollection)
        {
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, "App_Data", appDataToggleCollection);
            var toggleCollection = JsonConvert.DeserializeObject<ToggleCollection>(File.ReadAllText(file));

            return SetDataSource(toggleCollection);
        }

        public UnleashTestConfig SetDataSource(ToggleCollection collection)
        {
            InMemoryTogglesForUnitTestingPurposes = collection;

            return this;
        }
    }
}