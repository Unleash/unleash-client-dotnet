using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Unleash.Tests.Specifications.TestFactory;
using Unleash.Tests.Mock;
using Unleash.Internal;
using Unleash.Scheduling;
using System.Threading;
using Unleash.Variants;

namespace Unleash.Tests.Internal
{
    public class ImpressionData_Tests
    {
        [Test]
        public void Impression_Event_Gets_Called_For_IsEnabled()
        {
            // Arrange
            ImpressionEvent callbackEvent = null;
            var appname = "testapp";
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, true, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            result.Should().BeTrue();
            callbackEvent.Should().NotBeNull();
            callbackEvent.Enabled.Should().BeTrue();
            callbackEvent.Context.AppName.Should().Be(appname);
            callbackEvent.Variant.Should().BeNull();
        }

        [Test]
        public void Impression_Event_Does_Not_Get_Called_When_Not_Opted_In()
        {
            // Arrange
            ImpressionEvent callbackEvent = null;
            var appname = "testapp";
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, false, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            result.Should().BeTrue();
            callbackEvent.Should().BeNull();
        }

        [Test]
        public void Impression_Event_Callback_Invoker_Catches_Exception()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, true, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { throw new Exception("Something bad just happened!"); };
            });

            // Act, Assert
            Assert.DoesNotThrow(() => { unleash.IsEnabled("item"); });
            unleash.Dispose();
        }

        [Test]
        public void Impression_Event_Callback_Null_Does_Not_Throw()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, true, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = null;
            });

            // Act, Assert
            Assert.DoesNotThrow(() => { unleash.IsEnabled("item"); });
            unleash.Dispose();
        }

        [Test]
        public void Impression_Event_Gets_Called_For_Variants()
        {
            // Arrange
            ImpressionEvent callbackEvent = null;
            var appname = "testapp";
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var payload = new Payload { PayloadType = "string", Value = "val1" };
            var variant = new VariantDefinition("blue", 100, payload);
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, true, new List<ActivationStrategy>() { strategy }, new List<VariantDefinition>() { variant })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);
            unleash.ConfigureEvents(cfg =>
            {
                cfg.ImpressionEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.GetVariant("item");
            unleash.Dispose();

            // Assert
            result.Name.Should().Be("blue");
            callbackEvent.Should().NotBeNull();
            callbackEvent.Enabled.Should().BeTrue();
            callbackEvent.Variant.Should().Be("blue");
            callbackEvent.Context.AppName.Should().Be(appname);
        }

        [Test]
        public void Unhooked_Impression_Events_Doesnt_Cause_Everything_To_Fail()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") });
            var payload = new Payload { PayloadType = "string", Value = "val1" };
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("yup", "release", true, true, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var enabled = unleash.IsEnabled("yup");
            unleash.Dispose();

            // Assert
            enabled.Should().BeTrue();
        }

        public static IUnleash CreateUnleash(string name, ToggleCollection state)
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
