using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Serialization;
using Unleash.Tests.Mock;

namespace Unleash.Tests.Internal
{
    public class TogglesUpdatedEvent_Tests
    {
        [Test]
        public void TogglesUpdated_Event_Gets_Called_For_HasChanged_True()
        {
            // Arrange
            TogglesUpdatedEvent callbackEvent = null;
            var callbackConfig = new EventCallbackConfig()
            {
                TogglesUpdatedEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult(new FetchTogglesResult() { HasChanged = true, ToggleCollection = new ToggleCollection(), Etag = "one" }));

            var collection = new ThreadSafeToggleCollection();
            var serializer = new DynamicNewtonsoftJsonSerializer();
            serializer.TryLoad();

            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt");

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            callbackEvent.Should().NotBeNull();
        }

        [Test]
        public void TogglesUpdated_Event_Does_Not_Get_Called_For_HasChanged_False()
        {
            // Arrange
            TogglesUpdatedEvent callbackEvent = null;
            var callbackConfig = new EventCallbackConfig()
            {
                TogglesUpdatedEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult(new FetchTogglesResult() { HasChanged = false, ToggleCollection = new ToggleCollection(), Etag = "one" }));

            var collection = new ThreadSafeToggleCollection();
            var serializer = new DynamicNewtonsoftJsonSerializer();
            serializer.TryLoad();

            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt");

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            callbackEvent.Should().BeNull();
        }
    }
}
