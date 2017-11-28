using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Unleash.Repository;

namespace Unleash.Tests
{
    public class UnleashTestConfig : UnleashSettings
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
            AppName = "test";
            InstanceTag = "test instance 1";
            UnleashApi = new Uri("http://localhost:4242/");
            SetDataSource(Toggles);

            UnleashContextProvider = new DefaultUnleashContextProvider(
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
            //InMemoryTogglesForUnitTestingPurposes = collection;

            return this;
        }
    }
}