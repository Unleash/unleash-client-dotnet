using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using static Unleash.Tests.Specifications.TestFactory;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Unleash.Internal;
using Unleash.Scheduling;

using Unleash.Tests.Mock;
using Unleash.Variants;

namespace Unleash.Tests.Variants
{
	public class StrategyVariantTests
	{
		[Test]
		public void Picks_Strategy_Variant()
		{
            // Arrange
            var appname = "test";
            var toggles = new List<FeatureToggle>
            {
                new FeatureToggle(
                    "item",
                    "release",
                    true,
                    false,
                    new List<ActivationStrategy>
                    {
                        new ActivationStrategy(
                            "flexibleRollout",
                            new Dictionary<string, string>
                            {
                                { "rollout", "100" },
                                { "groupId", "grp" }
                            },
                            new List<Constraint>
                                { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") },
                            null,
                            new List<VariantDefinition>
                            {
                                new VariantDefinition("Red", 50, new Payload("text", "red")),
                                new VariantDefinition("Blue", 50, new Payload("text", "blue"))
                        }
                    ) },
                    new List<VariantDefinition>
                    {
                        new VariantDefinition("Green", 50, new Payload("text", "green")),
                        new VariantDefinition("Black", 50, new Payload("text", "black"))
                    })
                };

            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var variant = unleash.GetVariant("item");

            // Assert
            new[] { "Red", "Blue" }.Should().Contain(variant.Name);
        }

        [Test]
        public void Picks_Toggle_Variant_When_No_Strategy_Variants()
        {
            // Arrange
            var appname = "test";
            var toggles = new List<FeatureToggle>
            {
                new FeatureToggle(
                    "item",
                    "release",
                    true,
                    false,
                    new List<ActivationStrategy>
                    {
                        new ActivationStrategy(
                            "flexibleRollout",
                            new Dictionary<string, string>
                            {
                                { "rollout", "100" },
                                { "groupId", "grp" }
                            },
                            new List<Constraint>
                                { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") },
                            null,
                            null
                    )},
                    new List<VariantDefinition>
                    {
                        new VariantDefinition("Green", 50, new Payload("text", "green")),
                        new VariantDefinition("Black", 50, new Payload("text", "black"))
                    })
                };

            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var variant = unleash.GetVariant("item");

            // Assert
            new[] { "Green", "Black" }.Should().Contain(variant.Name);
        }

        [Test]
        public void Returns_Default_Variant_When_No_Variants()
        {
            // Arrange
            var appname = "test";
            var toggles = new List<FeatureToggle>
            {
                new FeatureToggle(
                    "item",
                    "release",
                    true,
                    false,
                    new List<ActivationStrategy>
                    {
                        new ActivationStrategy(
                            "flexibleRollout",
                            new Dictionary<string, string>
                            {
                                { "percentage", "100" },
                                { "groupId", "grp" }
                            },
                            new List<Constraint>
                                { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") },
                            null,
                            null
                        )
                    }
                )
            };

            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var variant = unleash.GetVariant("item");

            // Assert
            variant.Name.Should().Be("disabled");
        }

        [Test]
        public void Picks_Feature_Variant_When_First_Strategy_Has_No_Variants()
        {
            // Arrange
            var appname = "test";
            var toggles = new List<FeatureToggle>
            {
                new FeatureToggle(
                    "item",
                    "release",
                    true,
                    false,
                    new List<ActivationStrategy>
                    {
                        new ActivationStrategy(
                            "flexibleRollout",
                            new Dictionary<string, string>
                            {
                                { "rollout", "100" },
                                { "groupId", "grp1" }
                            }
                        ),
                        new ActivationStrategy(
                            "flexibleRollout",
                            new Dictionary<string, string>
                            {
                                { "rollout", "100" },
                                { "groupId", "grp2" }
                            },
                            new List<Constraint>
                                { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") },
                            null,
                            new List<VariantDefinition>
                            {
                                new VariantDefinition("Red", 50, new Payload("text", "red")),
                                new VariantDefinition("Blue", 50, new Payload("text", "blue"))
                        }
                    ) },
                    new List<VariantDefinition>
                    {
                        new VariantDefinition("Green", 50, new Payload("text", "green")),
                        new VariantDefinition("Black", 50, new Payload("text", "black"))
                    })
                };

            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var variant = unleash.GetVariant("item");

            // Assert
            new[] { "Green", "Black" }.Should().Contain(variant.Name);
        }

        public static IUnleash CreateUnleash(string name, ToggleCollection state)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>;
            var fakeHttpMessageHandler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var fakeScheduler = A.Fake<IUnleashScheduledTaskManager>;
            var fakeFileSystem = new MockFileSystem();
            var toggleState = Newtonsoft.Json.JsonConvert.SerializeObject(state);

            A.CallTo(() => fakeHttpClientFactory.Create(A<Uri>._)).Returns(httpClient);
            A.CallTo(() => fakeScheduler.Configure(A<IEnumerable<IUnleashScheduledTask>>._, A<CancellationToken>._)).Invokes(action =>
            {
                var task = ((IEnumerable<IUnleashScheduledTask>)action.Arguments[0]).First();
                task.ExecuteAsync((CancellationToken)action.Arguments[1]).Wait();
            });

            fakeHttpMessageHandler.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(toggleState, Encoding.UTF8, "application/json"),
                Headers =
                {
                    ETag = new EntityTagHeaderValue("\"123\"")
                }
            };

            var contextBuilder = new UnleashContext.Builder();
            contextBuilder.AddProperty("item-id", "1");

            var settings = new UnleashSettings
            {
                AppName = name,
                UnleashContextProvider = new DefaultUnleashContextProvider(contextBuilder.Build()),
                HttpClientFactory = fakeHttpClientFactory,
                ScheduledTaskManager = fakeScheduler,
                FileSystem = fakeFileSystem
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }
    }
}

