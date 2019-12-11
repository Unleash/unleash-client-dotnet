using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.DynamicProxy.Generators.Emitters;
using Unleash.Internal;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Core.Tests.Serialization
{
    public abstract class BaseJsonSerializerTests<TSerializer>
        where TSerializer : class, IJsonSerializer
    {
        public abstract TSerializer CreateSerializer();

        protected ToggleCollection CreateToggleCollection() => new ToggleCollection(
            new List<FeatureToggle>
            {
                new FeatureToggle(
                    "Feature1",
                    true,
                    new List<ActivationStrategy>
                    {
                        new ActivationStrategy(
                            "remoteAddress",
                            new Dictionary<string, string>
                            {
                                {"IPs", "127.0.0.1"}
                            })
                    }),
                new FeatureToggle(
                    "feature2",
                    false,
                    new List<ActivationStrategy>
                    {
                        new ActivationStrategy("userWithId", new Dictionary<string, string>()
                        {
                            {"userIds", "james"}
                        })
                    })
            });

        protected string[] ExpectedQuotedTokens { get; } = {
            "Feature1",
            "remoteAddress",
            "IPs",
            "127.0.0.1",

            "feature2",
            "userWithId",
            "userIds",
            "james",
        };

        protected string[] ExpectedUnquotedTokens { get; } = {
            "true",
            "false"
        };

        protected string SerializeToggleCollection(ToggleCollection toggleCollection)
        {
            var serializer = CreateSerializer();
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, toggleCollection);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        protected ToggleCollection DeserializeToggleCollection(string json)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 16384, true))
                {
                    streamWriter.Write(json);
                }

                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);

                var serializer = CreateSerializer();
                return serializer.Deserialize<ToggleCollection>(memoryStream);
            }
        }

        [Fact]
        public void Serialize_WhenInvoked_ResultShouldContainExpectedTokens()
        {
            var toggleCollection = CreateToggleCollection();
            var json= SerializeToggleCollection(toggleCollection);

            foreach (var token in ExpectedQuotedTokens)
            {
                Assert.True(json.IndexOf("\"" + token + "\"", StringComparison.InvariantCulture) >= 0);
            }

            foreach (var token in ExpectedUnquotedTokens)
            {
                Assert.True(json.IndexOf(token, StringComparison.InvariantCulture) >= 0);
            }
        }

        [Fact]
        public void SerializeAndDeserialize_WhenInvokedTogether_ReturnPredictableResults()
        {
            var originalToggleCollection = CreateToggleCollection();
            var originalJson= SerializeToggleCollection(originalToggleCollection);
            var deserializedToggleCollection = DeserializeToggleCollection(originalJson);
            var serializedJson = SerializeToggleCollection(deserializedToggleCollection);

            Assert.Equal(originalJson, serializedJson);
        }

        [Fact]
        public void SerializeAndDeserialize_WhenInvokedTogether_ProducesExpectedToggleCollection()
        {
            var originalToggleCollection = CreateToggleCollection();
            var originalJson= SerializeToggleCollection(originalToggleCollection);
            var deserializedToggleCollection = DeserializeToggleCollection(originalJson);

            var features = deserializedToggleCollection.Features.ToArray();

            Assert.Equal(2, features.Length);

            AssertFeatures(
                features[0],
                "Feature1",
                true,
                "remoteAddress",
                "IPs",
                "127.0.0.1");

            AssertFeatures(
                features[1],
                "feature2",
                false,
                "userWithId",
                "userIds",
                "james");
        }

        private void AssertFeatures(
            FeatureToggle feature,
            string expectedFeatureName,
            bool expectedEnabled,
            string expectedStrategyName,
            string expectedParameterKey,
            string expectedParameterValue)
        {
            Assert.Equal(expectedFeatureName, feature.Name);
            Assert.Equal(expectedEnabled, feature.Enabled);
            Assert.Single(feature.Strategies);

            var featureStrategy = feature.Strategies[0];

            Assert.Equal(expectedStrategyName, featureStrategy.Name);
            Assert.Single(featureStrategy.Parameters);

            var featureStrategyParameter = featureStrategy.Parameters.First();

            Assert.Equal(expectedParameterKey, featureStrategyParameter.Key);
            Assert.Equal(expectedParameterValue, featureStrategyParameter.Value);
        }
    }
}
