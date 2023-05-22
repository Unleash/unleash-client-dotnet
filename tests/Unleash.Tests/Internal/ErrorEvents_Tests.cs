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
            callbackEvent.Error.Should().BeNull();
            callbackEvent.ErrorType.Should().Be(ErrorType.Client);
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
            callbackEvent.Error.Should().BeNull();
            callbackEvent.ErrorType.Should().Be(ErrorType.Client);
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
            callbackEvent.Error.Should().NotBeNull();
            callbackEvent.ErrorType.Should().Be(ErrorType.Client);
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
            callbackEvent.Error.Should().NotBeNull();
            callbackEvent.Error.Message.Should().Be(exceptionMessage);
            callbackEvent.ErrorType.Should().Be(ErrorType.TogglesBackup);
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
            callbackEvent.Error.Should().NotBeNull();
            callbackEvent.Error.Message.Should().Be(exceptionMessage);
            callbackEvent.ErrorType.Should().Be(ErrorType.TogglesBackup);
        }

        [Test]
        public void CachedFilesLoader_Raises_ErrorEvent()
        {
            // Arrange
            ErrorEvent callbackEvent = null;
            var callbackConfig = new EventCallbackConfig()
            {
                ErrorEvent = evt => { callbackEvent = evt; }
            };

            var serializer = A.Fake<IJsonSerializer>();

            var exceptionMessage = "Writing failed";
            var filesystem = A.Fake<IFileSystem>();
            A.CallTo(() => filesystem.WriteAllText(A<string>._, A<string>._))
                .Throws(() => new IOException(exceptionMessage));

            var toggleBootstrapProvider = A.Fake<IToggleBootstrapProvider>();

            var filecache = new CachedFilesLoader(serializer, filesystem, toggleBootstrapProvider, callbackConfig, "toggle.txt", "etag.txt");

            // Act
            filecache.EnsureExistsAndLoad();

            // Assert
            callbackEvent.Should().NotBeNull();
            callbackEvent.Error.Should().NotBeNull();
            callbackEvent.ErrorType.Should().Be(ErrorType.FileCache);
        }
    }
}
