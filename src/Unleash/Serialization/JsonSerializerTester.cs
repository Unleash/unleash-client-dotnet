using System.Collections.Generic;
using Unleash.Repository;

namespace Unleash.Serialization
{
    /// <summary>
    /// Helper class for verifying that custom implemented serializers work as expected.
    /// </summary>
    public static class JsonSerializerTester
    {
        private static readonly ToggleCollection Toggles = new ToggleCollection(new List<FeatureToggle>
        {
            new FeatureToggle("one", true, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userByName", new Dictionary<string, string>(){
                    {"Demo", "Demo" }
                })
            }),
            new FeatureToggle("two", false, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userByName2", new Dictionary<string, string>()
                {
                    {"demo", "demo" }
                })
            })
        });

        public static void Assert(IJsonSerializer serializer)
        {
            var json = serializer.Serialize(Toggles).ConvertToString();

            ShouldContain(json, "\"name\":\"one\"");

            ShouldContain(json, "\"Demo\":\"Demo\"");
            ShouldContain(json, "\"demo\":\"demo\"");

            ToggleCollection toggleCollection;
            using (var jsonStream = json.ConvertToStream())
            {
                toggleCollection = serializer.Deserialize<ToggleCollection>(jsonStream);
            }

            ShouldBeEqual(Toggles, toggleCollection);
        }

        private static void ShouldBeEqual(ToggleCollection collection1, ToggleCollection collection2)
        {
            if (collection1.Features.Count != collection2.Features.Count)
                throw new UnleashException("Number of elements are different.");
        }

        private static void ShouldContain(string json, string fragment)
        {
            var valid = json.Contains(fragment);
            if(!valid)
                throw new UnleashException("Json not properly formatted.");
        }
    }
}