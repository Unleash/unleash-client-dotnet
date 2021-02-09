using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unleash.Internal;

namespace Unleash.Serialization
{
    /// <summary>
    /// Helper class for verifying that custom implemented json serializers work as expected.
    /// </summary>
    public static class JsonSerializerTester
    {
        private static readonly ToggleCollection Toggles = new ToggleCollection(new List<FeatureToggle>
        {
            new FeatureToggle("Feature1", "release", true, new List<ActivationStrategy>()
            {
                new ActivationStrategy("remoteAddress", new Dictionary<string, string>()
                {
                    {"IPs", "127.0.0.1"}
                })
            }),
            new FeatureToggle("feature2", "release", false, new List<ActivationStrategy>()
            {
                new ActivationStrategy("userWithId", new Dictionary<string, string>()
                {
                    {"userIds", "james"}
                })
            })
        });

        public static void Assert(IJsonSerializer serializer)
        {
            var sb = new StringBuilder();

            string json;
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, Toggles);
                json = memoryStream.ConvertToString();
            }

            var shouldContainTokens = new List<string>
            {
                "Feature1",
                "true",
                "remoteAddress",
                "IPs",
                "127.0.0.1",

                "feature2",
                "false",
                "userWithId",
                "userIds",
                "james",
            };

            foreach (var token in shouldContainTokens)
            {
                if (json.IndexOf(token, StringComparison.InvariantCulture) < 0)
                {
                    sb.AppendLine($"Error: Could not locate element: '{token}'");
                }
            }

            if (sb.Length > 0)
                throw new UnleashException($"Serialization errors occurred:{Environment.NewLine}{sb}");


            // Deserialization
            using (var jsonStream = json.ConvertToStream())
            {
                var toggleCollection = serializer.Deserialize<ToggleCollection>(jsonStream);

                string errorMessage;
                using (var ms = new MemoryStream())
                {
                    serializer.Serialize(ms, toggleCollection);
                    var actual = ms.ConvertToString();
                    errorMessage = $"Expected: {json}. Actual: {actual}";

                    if (!json.Equals(actual))
                        throw new UnleashException(errorMessage);
                }

                if (toggleCollection.Features.Count != 2)
                    throw new UnleashException($"Different # of features: {errorMessage}");

                var feature1 = toggleCollection.Features.First();

                if (feature1.Name != "Feature1")
                    throw new UnleashException($"Wrong Feature1 name: {errorMessage}");

                if (feature1.Strategies.Count != 1)
                    throw new UnleashException($"Wrong stragegies count: {errorMessage}");

                if (feature1.Strategies[0].Name != "remoteAddress")
                    throw new UnleashException($"Wrong expected strategy name (remoteAddress): {errorMessage}");


                if (feature1.Strategies[0].Parameters.Count != 1)
                    throw new UnleashException($"Wrong expected strategy parameters count (1): {errorMessage}");

                var keyValuePair = feature1.Strategies[0].Parameters.First();
                if (keyValuePair.Key != "IPs")
                    throw new UnleashException($"Wrong expected strategy parameters key (IPs): {errorMessage}");

                if (keyValuePair.Value != "127.0.0.1")
                    throw new UnleashException($"Wrong expected strategy parameters value (127.0.0.1): {errorMessage}");
            }
        }
   }
}