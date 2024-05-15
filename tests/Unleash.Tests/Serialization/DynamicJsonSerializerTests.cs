using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Unleash.Serialization;
using Unleash.Internal;
using System.Linq;
using System.Collections.ObjectModel;

namespace Unleash.Tests.Serialization
{
    public class DynamicJsonSerializerTests : BaseTest
    {
        [Test]
        public void Asserter()
        {
            JsonSerializerTester.Assert(new JsonNetSerializer());

            var serializer = new DynamicNewtonsoftJsonSerializer();
            serializer.TryLoad();
            JsonSerializerTester.Assert(serializer);

            JsonSerializerTester.Assert(new SystemTextJsonSerializer());
        }

        [TestCase(typeof(JsonNetSerializer))]
        [TestCase(typeof(DynamicNewtonsoftJsonSerializer))]
        [TestCase(typeof(SystemTextJsonSerializer))]
        public void Deserialize_SameAs_NewtonSoft(Type type)
        {
            Console.WriteLine(type.FullName);
            Console.WriteLine();

            var path = AppDataFile("features-v1.json");
            var originalJson = File.ReadAllText(path);
            var expected = JsonConvert.DeserializeObject<ToggleCollection>(originalJson);

            var serializer = (IDynamicJsonSerializer) Activator.CreateInstance(type);
            serializer.TryLoad().Should().BeTrue();

            using (var fileStream = File.OpenRead(path))
            {
                var toggleCollection = serializer.Deserialize<ToggleCollection>(fileStream);

                toggleCollection.Should().BeEquivalentTo(expected);
            }
        }

        [TestCase(typeof(JsonNetSerializer))]
        [TestCase(typeof(DynamicNewtonsoftJsonSerializer))]
        [TestCase(typeof(SystemTextJsonSerializer))]
        public void Serialize_SameAsNewtonSoft(Type type)
        {
            Console.WriteLine(type.FullName);
            Console.WriteLine();

            var collection = new ToggleCollection(new List<FeatureToggle>()
            {
                new FeatureToggle("one",  "release", true, false, new List<ActivationStrategy>()
                {
                    new ActivationStrategy("userByName", new Dictionary<string, string>(){
                        {"Demo", "Demo" }
                    })
                }),
                new FeatureToggle("two",  "release", false, false, new List<ActivationStrategy>()
                {
                    new ActivationStrategy("userByName2", new Dictionary<string, string>()
                    {
                        {"demo", "demo" }
                    })
                })
            });

            var serializer = (IDynamicJsonSerializer) Activator.CreateInstance(type);
            serializer.TryLoad()
                .Should().BeTrue();

            var expected = JsonConvert.SerializeObject(collection, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                    {
                        ProcessDictionaryKeys = false,
                    }
                }
            });

            expected.Should().Contain("\"Demo\":\"Demo\"");
            expected.Should().Contain("\"demo\":\"demo\"");

            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, collection);

                var resultingJson = ms.ConvertToString();
                Console.WriteLine(resultingJson);

                resultingJson.Should().Contain("\"Demo\":\"Demo\"");
                resultingJson.Should().Contain("\"demo\":\"demo\"");

                resultingJson.Should().Be(expected);
            }
        }

        [Test]
        public void Deserializes_ImpressionData_Property()
        {
            // Arrange
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "App_Data", "impressiondata-v2.json");
            var originalJson = File.ReadAllText(path);

            // Act
            var deserialized = JsonConvert.DeserializeObject<ToggleCollection>(originalJson);

            // Assert
            var toggle = deserialized.Features.First();
            toggle.Should().NotBeNull();
            toggle.ImpressionData.Should().BeTrue();
        }

        [Test]
        public void Serializes_ImpressionData_Property()
        {
            // Arrange
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, true, new List<ActivationStrategy>() { strategy })
            };

            var state = new ToggleCollection(toggles);
            state.Version = 2;

            // Act
            var serialized = JsonConvert.SerializeObject(toggles, new JsonSerializerSettings());

            // Assert
            var contains = serialized.IndexOf("\"ImpressionData\":true") >= 0;
            contains.Should().BeTrue();
        }

        [Test]
        public void Dependent_Feature_Enabled_Defaults_To_True() {
            // Arrange
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "App_Data", "dependent-features-missing-enabled.json");
            var originalJson = File.ReadAllText(path);

            // Act
            var deserialized = JsonConvert.DeserializeObject<ToggleCollection>(originalJson);
            var toggle = deserialized.Features.First(f => f.Name == "enabled-child");

            // Assert
            toggle.Should().NotBeNull();
            toggle.Dependencies.Should().NotBeEmpty();
            toggle.Dependencies.First().Enabled.Should().BeTrue();
        }
    }
}