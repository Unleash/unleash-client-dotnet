using FakeItEasy;
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
using Unleash.Scheduling;
using System.Threading;
using Unleash.Internal;
using Unleash.Events;
using Unleash.Communication;
using Unleash.Serialization;
using FluentAssertions;
using Unleash.Metrics;
using System.IO;

namespace Unleash.Tests.Internal
{
    public class ErrorEvents_Tests
    {
        [Test]
        public void Fetch_Toggles_Unauthorized_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
            var fakeHttpMessageHandler = new TestHttpMessageHandler()
            {
                Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Unauthorized", Encoding.UTF8) },
            };
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var callbackConfig = new EventCallbackConfig()
            {
                ErrorEvent = evt => { callbackEvent = evt; }
            };
            var unleashClient = new UnleashApiClient(httpClient, new DynamicNewtonsoftJsonSerializer(), new UnleashApiClientRequestHeaders(), eventConfig: callbackConfig);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = unleashClient.FetchToggles("123", cancellationTokenSource.Token).Result;

            // Assert
            callbackEvent.Should().NotBeNull();
        }

        [Test]
        public void RegisterClient_Unauthorized_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
            var fakeHttpMessageHandler = new TestHttpMessageHandler()
            {
                Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Unauthorized", Encoding.UTF8) },
            };
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            var callbackConfig = new EventCallbackConfig()
            {
                ErrorEvent = evt => { callbackEvent = evt; }
            };

            var deserializer = new DynamicNewtonsoftJsonSerializer();
            deserializer.TryLoad();
            var unleashClient = new UnleashApiClient(httpClient, deserializer, new UnleashApiClientRequestHeaders(), eventConfig: callbackConfig);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = unleashClient.RegisterClient(new ClientRegistration(), cancellationTokenSource.Token).Result;

            // Assert
            callbackEvent.Should().NotBeNull();
        }

        [Test]
        public void FetchFeatureToggleTask_HttpRequestException_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
            var callbackConfig = new EventCallbackConfig()
            {
                ErrorEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._))
                .ThrowsAsync(() => new HttpRequestException("The remote server refused the connection"));

            var collection = new ThreadSafeToggleCollection();
            var serializer = new DynamicNewtonsoftJsonSerializer();
            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt");

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            callbackEvent.Should().NotBeNull();
        }

        [Test]
        public void FetchFeatureToggleTask_Serialization_Throws_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
            var exceptionMessage = "Serialization failed";
            var callbackConfig = new EventCallbackConfig()
            {
                ErrorEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult(new FetchTogglesResult() { HasChanged = true, ToggleCollection = new ToggleCollection(), Etag = "one" }));

            var collection = new ThreadSafeToggleCollection();
            var serializer = A.Fake<IJsonSerializer>();
            A.CallTo(() => serializer.Serialize(A<Stream>._, A<ToggleCollection>._))
                .Throws(() => new IOException(exceptionMessage));

            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt");

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            callbackEvent.Should().NotBeNull();
            callbackEvent.Error.Message.Should().Be(exceptionMessage);
        }

        [Test]
        public void FetchFeatureToggleTask_Etag_Writing_Throws_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
            var exceptionMessage = "Writing failed";
            var callbackConfig = new EventCallbackConfig()
            {
                ErrorEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult(new FetchTogglesResult() { HasChanged = true, ToggleCollection = new ToggleCollection(), Etag = "one" }));

            var collection = new ThreadSafeToggleCollection();
            var serializer = A.Fake<IJsonSerializer>();

            var filesystem = A.Fake<IFileSystem>();
            A.CallTo(() => filesystem.WriteAllText(A<string>._, A<string>._))
                .Throws(() => new IOException(exceptionMessage));

            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt");

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            callbackEvent.Should().NotBeNull();
            callbackEvent.Error.Message.Should().Be(exceptionMessage);
        }

        [Test]
        public void DefaultUnleash_ImpressionEvent_Callback_Exception_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
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
                cfg.ImpressionEvent = evt => { throw new Exception("Something went wrong in handling this callback"); };
                cfg.ErrorEvent = evt => { callbackEvent = evt; };
            });

            // Act
            var result = unleash.IsEnabled("item");
            unleash.Dispose();

            // Assert
            result.Should().BeTrue();
            callbackEvent.Should().NotBeNull();
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
