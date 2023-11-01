using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Linq;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Strategies;
using Unleash.Tests.Mock;
using Unleash.Variants;
using static Unleash.Tests.Specifications.TestFactory;
using System.Text.Json;
using Unleash.ClientFactory;

namespace Unleash.Tests
{
    public class DefaultUnleashTests
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Test]
        public void Configure_Http_Client_Factory()
        {
            // Arrange
            var factory = new HttpClientFactoryMock();
            var apiUri = new Uri("http://localhost:8080/");

            // Act
            var client = factory.Create(apiUri);

            // Assert
            factory.CreateHttpClientInstanceCalled.Should().BeTrue();
        }

        [Test]
        public async Task IsEnabled_Flexible_Strategy_Test()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("flexibleRollout", new Dictionary<string, string>() { { "rollout", "100" } }, new List<Constraint>() { });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("test_toggle", "experimental", true, false, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = await CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("test_toggle");

            // Assert
            result.Should().BeTrue();

            unleash.Dispose();
        }

        [Test]
        public async Task IsEnabled_Flexible_Strategy_Test2()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("flexibleRollout", new Dictionary<string, string>() { { "rollout", "100" } }, new List<Constraint>() { });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("test_toggle", "experimental", true, false, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = await CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("test_toggle");

            // Assert
            result.Should().BeTrue();

            unleash.Dispose();
        }

        [Test]
        public async Task IsEnabled_Flexible_Strategy_Multi_Test()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("flexibleRollout", new Dictionary<string, string>() { { "rollout", "100" } }, new List<Constraint>() { });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("test_toggle", "experimental", true, false, new List<ActivationStrategy>() { strategy })
            };

            var state = new ToggleCollection(toggles);
            state.Version = 2;

            var unleash1 = await CreateUnleash(appname, state);
            var result1 = unleash1.IsEnabled("test_toggle");
            unleash1.Dispose();

            var unleash2 = await CreateUnleash(appname, state);
            var result2 = unleash2.IsEnabled("test_toggle");
            unleash2.Dispose();

            var unleash3 = await CreateUnleash(appname, state);
            var result3 = unleash3.IsEnabled("test_toggle");
            unleash3.Dispose();

            var unleash4 = await CreateUnleash(appname, state);
            var result4 = unleash4.IsEnabled("test_toggle");
            unleash4.Dispose();

            // Act

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            result3.Should().BeTrue();
            result4.Should().BeTrue();

            //unleash.Dispose();
        }

        [Test]
        public async Task IsEnabled_Gradual_Rollout_Random_Strategy_Test()
        {
            // Arrange
            var appname = "testapp";
            var strategy = new ActivationStrategy("gradualRolloutRandom", new Dictionary<string, string>() { { "percentage", "100" } }, new List<Constraint>() { });
            var toggles = new List<FeatureToggle>()
            {
                new FeatureToggle("test_toggle", "experimental", true, false, new List<ActivationStrategy>() { strategy })
            };


            var state = new ToggleCollection(toggles);
            state.Version = 2;
            var unleash = await CreateUnleash(appname, state);

            // Act
            var result = unleash.IsEnabled("test_toggle");

            // Assert
            result.Should().BeTrue();

            unleash.Dispose();
        }

        public static async Task<IUnleash> CreateUnleash(string name, ToggleCollection state)
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

            var settings = new UnleashSettings
            {
                AppName = name,
                HttpClientFactory = fakeHttpClientFactory,
            };

            var unleash = await new UnleashClientFactory()
                .CreateClientAsync(settings, synchronousInitialization: true);

            return unleash;
        }
    }
}
