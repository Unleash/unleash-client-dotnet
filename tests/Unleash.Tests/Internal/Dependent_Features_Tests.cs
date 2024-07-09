using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Scheduling;
using Unleash.Tests.Mock;
using Unleash.Utilities;
using Unleash.Variants;
using static Unleash.Tests.Specifications.TestFactory;

namespace Unleash.Tests.Internal
{
    public class Dependent_Features_Tests
    {
        [Test]
        public void Warns_Once_For_Given_Key()
        {
            // Arrange
            var logger = A.Fake<ILog>();
            var warnOnce = new WarnOnce(logger);

            // Act
            warnOnce.Warn("test", "testmessage");
            warnOnce.Warn("test", "testmessage");

            // Assert
            A.CallTo(() => logger.Log(A<LogLevel>._, A<Func<string>>._, null)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Warns_Once_For_Each_Given_Key()
        {
            // Arrange
            var logger = A.Fake<ILog>();
            var warnOnce = new WarnOnce(logger);

            // Act
            warnOnce.Warn("test", "testmessage");
            warnOnce.Warn("test", "testmessage");

            warnOnce.Warn("test2", "testmessage2");
            warnOnce.Warn("test2", "testmessage2");

            // Assert
            A.CallTo(() => logger.Log(A<LogLevel>._, A<Func<string>>._, null)).MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void Depends_On_One_Enabled_Parent_IsEnabled_True()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Depends_On_One_Enabled_Parent_With_Variants_Red_Or_Blue_IsEnabled_True()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-variants-enabled-1", variants: new [] { "red", "blue" }),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentWithVariantsRedBlueEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Depends_On_One_Enabled_Parent_Counts_Metrics_Only_For_Child()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            unleash.services.MetricsBucket.StopCollectingMetrics(out var bucket);
            var childMetrics = bucket.Toggles.Single(t => t.Key == "child-1").Value;
            childMetrics.No.Should().Be(0L);
            childMetrics.Yes.Should().Be(1L);

            var parentMetrics = bucket.Toggles.Any(t => t.Key == "parent-enabled-1").Should().BeFalse();
        }

        [Test]
        public void Depends_On_One_Parent_With_No_Variants_Returns_Own_Variant()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
            };
            var childVariants = new List<VariantDefinition>()
            {
                new VariantDefinition("red", 50, new Payload("colour", "Red")),
                new VariantDefinition("blue", 50, new Payload("colour", "Blue")),
            };

            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies, variants: childVariants)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.GetVariant("child-1");

            // Assert
            result.Name.Should().BeOneOf("red", "blue");
        }

        [Test]
        public void Depends_On_One_Parent_With_Variant_Disabled_Returns_Own_Variant()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1", variants: new [] { "disabled" }),
            };
            var childVariants = new List<VariantDefinition>()
            {
                new VariantDefinition("red", 50, new Payload("colour", "Red")),
                new VariantDefinition("blue", 50, new Payload("colour", "Blue")),
            };

            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies, variants: childVariants)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.GetVariant("child-1");

            // Assert
            result.Name.Should().BeOneOf("red", "blue");
        }

        [Test]
        public void Depends_On_One_Enabled_Parent_Fires_Impression_Events_For_Both_Parend_And_Child()
        {
            // Arrange
            var appname = "testapp";
            var impressionEventCount = 0;
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(impressionData: true),
                ParentEnabledTwo(impressionData: true),
                ChildDependentOn("child-1", dependencies, impressionData: true)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = (ev) =>
                {
                    impressionEventCount++;
                };
            });

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            impressionEventCount.Should().Be(2);
        }

        [Test]
        public void Depends_On_One_Enabled_Parent_With_No_Variants_Expects_Red_Or_Blue_IsEnabled_False()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-variants-enabled-1", variants: new [] { "red", "blue" }),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Depends_On_One_Enabled_Parent_With_No_Variants_Expects_Disabled_IsEnabled_True()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1", variants: new [] { "disabled" }),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Depends_On_One_NotEnabled_Parent_With_No_Variants_Expects_Disabled_IsEnabled_False()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-not-enabled-1", variants: new [] { "disabled" }),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentNotEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Depends_On_One_Missing_Parent_IsEnabled_False()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-3"),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Depends_On_One_NotEnabled_Parent_IsEnabled_True()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-not-enabled-1", enabled: false),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ParentNotEnabledOne(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Depends_On_One_NotEnabled_And_One_Enabled_Parent_IsEnabled_True()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1", enabled: true),
                new Dependency("parent-not-enabled-1", enabled: false),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ParentNotEnabledOne(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Depends_On_Enabled_Being_NotEnabled_Returns_False()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1", enabled: false),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ParentNotEnabledOne(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Depends_On_Child_Returns_False()
        {
            // Arrange
            var appname = "testapp";
            var parentDependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
            };
            var dependencies = new List<Dependency>()
            {
                new Dependency("child-1"),
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ChildDependentOn("child-1", parentDependencies),
                ChildDependentOn("child-2", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-2");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Depends_On_Two_Enabled_Parents_IsEnabled_True()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
                new Dependency("parent-enabled-2")
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentEnabledTwo(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Depends_On_One_Enabled_And_One_Not_Enabled_Parent_IsEnabled_False()
        {
            // Arrange
            var appname = "testapp";
            var dependencies = new List<Dependency>()
            {
                new Dependency("parent-enabled-1"),
                new Dependency("parent-enabled-2")
            };
            var toggles = new List<FeatureToggle>()
            {
                ParentEnabledOne(),
                ParentNotEnabledOne(),
                ChildDependentOn("child-1", dependencies)
            };
            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("child-1");

            // Assert
            result.Should().BeFalse();
        }

        public static FeatureToggle ChildDependentOn(string name, List<Dependency> dependencies, bool impressionData = false, List<VariantDefinition>? variants = null)
        {
            return new FeatureToggle(name, "release", true, impressionData, OnlyFlexibleRollout100Pct(), dependencies: dependencies, variants: variants);
        }

        public static FeatureToggle ParentNotEnabledOne(bool impressionData = false)
        {
            return new FeatureToggle("parent-not-enabled-1", "release", true, impressionData, OnlyFlexibleRolloutNone());
        }

        public static FeatureToggle ParentEnabledOne(bool impressionData = false)
        {
            return new FeatureToggle("parent-enabled-1", "release", true, impressionData, OnlyFlexibleRollout100Pct());
        }

        public static FeatureToggle ParentWithVariantsRedBlueEnabledOne()
        {
            return new FeatureToggle("parent-variants-enabled-1", "release", true, false, OnlyFlexibleRollout100PctVariantsRedBlue());
        }

        public static FeatureToggle ParentEnabledTwo(bool impressionData = false)
        {
            return new FeatureToggle("parent-enabled-2", "release", true, impressionData, OnlyFlexibleRollout100Pct());
        }

        public static List<ActivationStrategy> OnlyFlexibleRollout100Pct(List<Constraint>? constraints = null)
        {
            return
                new List<ActivationStrategy>()
                {
                    new ActivationStrategy(
                        "flexibleRollout",
                        new Dictionary<string, string>() { { "rollout", "100" } },
                        constraints ?? new List<Constraint>() { }
                    )
                };
        }

        public static List<ActivationStrategy> OnlyFlexibleRollout100PctVariantsRedBlue(List<Constraint>? constraints = null)
        {
            return
                new List<ActivationStrategy>()
                {
                    new ActivationStrategy(
                        "flexibleRollout",
                        new Dictionary<string, string>() { { "rollout", "100" } },
                        constraints ?? new List<Constraint>() { },
                        variants: new List<VariantDefinition>()
                        {
                            new VariantDefinition("red", 50, new Payload("colour", "Red")),
                            new VariantDefinition("blue", 50, new Payload("colour", "Blue")),
                        }
                    )
                };
        }

        public static List<ActivationStrategy> OnlyFlexibleRolloutNone(List<Constraint>? constraints = null)
        {
            return
                new List<ActivationStrategy>()
                {
                    new ActivationStrategy(
                        "flexibleRollout",
                        new Dictionary<string, string>() { { "rollout", "0" } },
                        constraints ?? new List<Constraint>() { }
                    )
                };
        }

        public static DefaultUnleash CreateUnleash(string name, ToggleCollection state)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeHttpMessageHandler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var fakeScheduler = A.Fake<IUnleashScheduledTaskManager>();
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

            var settings = new UnleashSettings
            {
                AppName = name,
                HttpClientFactory = fakeHttpClientFactory,
                ScheduledTaskManager = fakeScheduler,
                FileSystem = fakeFileSystem,
                DisableSingletonWarning = true
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }
    }
}