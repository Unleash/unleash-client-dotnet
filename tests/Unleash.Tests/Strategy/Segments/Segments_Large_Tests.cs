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
    public class Segments_Large_Tests
    {
        //[Test]
        public void Run200k_ish_Constraints()
        {
            var segmentsCount = 1000;
            var constraintsPerSegment = 200;
            var appname = "masstest";

            Console.WriteLine("Starting");

            var watch = new Stopwatch();
            watch.Start();

            var segmentIds = Enumerable.Range(1, segmentsCount).Select(id => id.ToString()).ToList();

            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("item", "release", true, new List<ActivationStrategy>() { new ActivationStrategy("default", new Dictionary<string, string>(), null, segmentIds) })
            };
            var segments = segmentIds.Select(id => new Segment(id, Enumerable.Range(1, 200).Select(cId => new Constraint("item-id", Operator.NUM_EQ, false, false, "1")).ToList())).ToList();

            var state = new ToggleCollection(toggles, segments);
            state.Version = 2;

            // Arrange
            var unleash = CreateUnleash(appname, state);

            Console.WriteLine("Setup took " + watch.Elapsed);

            watch.Reset();
            watch.Start();

            // Act
            var result = unleash.IsEnabled("item");

            Console.WriteLine("Running took " + watch.Elapsed);

            watch.Stop();

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
