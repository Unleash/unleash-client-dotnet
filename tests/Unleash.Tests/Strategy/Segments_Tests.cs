using FakeItEasy;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Tests.Mock;
using static Unleash.Tests.Specifications.TestFactory;

namespace Unleash.Tests.Strategy.Segments
{
    public class Segments_Tests
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            
        };

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

        public static IUnleash CreateUnleash(string name, ToggleCollection state)
        {
            var fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeHttpMessageHandler = new TestHttpMessageHandler();
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var toggleState = JsonSerializer.Serialize(state, options);

            A.CallTo(() => fakeHttpClientFactory.Create(A<Uri>._)).Returns(httpClient);

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
            };

            var unleash = new DefaultUnleash(settings);

            return unleash;
        }
    }
}