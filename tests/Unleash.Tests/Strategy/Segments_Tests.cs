using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Tests.Mock;
using static Unleash.Tests.Specifications.TestFactory;

namespace Unleash.Tests.Strategy.Segments
{
    public class Segments_Tests
    {
        [Test]
        public void Two_Constraints_With_Item_Id_Equals_1_And_Context_Item_Id_Equals_1_Should_Eval_To_True()
        {
            // Arrange
            var appname = "masstest";
            var segmentIds = new List<int>() { 1, 2 };
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, false, new List<ActivationStrategy>() { new ActivationStrategy("default", new Dictionary<string, string>(), null, segmentIds) })
            };

            var segments = segmentIds.Select(id => new Segment(id, new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") })).ToList();

            var state = new ToggleCollection(toggles, segments);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Two_Constraints_One_Correct_In_Segment_One_Wrong_In_Strategy_Should_Eval_To_False()
        {
            // Arrange
            var appname = "masstest";
            var segmentIds = new List<int>() { 1 };
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, false, new List<ActivationStrategy>() { new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "15") }, segmentIds) })
            };

            var segments = segmentIds.Select(id => new Segment(id, new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") })).ToList();

            var state = new ToggleCollection(toggles, segments);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void Two_Constraints_One_In_Segment_One_In_Strategy_Both_Correct_Should_Eval_To_True()
        {
            // Arrange
            var appname = "masstest";
            var segmentIds = new List<int>() { 1 };
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, false, new List<ActivationStrategy>() { new ActivationStrategy("default", new Dictionary<string, string>(), new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") }, segmentIds) })
            };

            var segments = segmentIds.Select(id => new Segment(id, new List<Constraint>() { new Constraint("item-id", Operator.NUM_EQ, false, false, "1") })).ToList();

            var state = new ToggleCollection(toggles, segments);
            state.Version = 2;
            var unleash = CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            Assert.AreEqual(true, result);
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