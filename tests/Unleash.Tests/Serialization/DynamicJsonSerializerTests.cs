using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Unleash.Serialization;
using Unleash.Internal;

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
        }

        [TestCase(typeof(JsonNetSerializer))]
        [TestCase(typeof(DynamicNewtonsoftJsonSerializer))]
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
        public void Serialize_SameAsNewtonSoft(Type type)
        {
            Console.WriteLine(type.FullName);
            Console.WriteLine();

            var collection = new ToggleCollection(new List<FeatureToggle>()
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
    }
}