using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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
            var callbackConfig = new EventCallbackConfig
            {
                TogglesUpdatedEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._, false))
                .Returns(Task.FromResult(new FetchTogglesResult { HasChanged = true, ToggleCollection = new ToggleCollection(), Etag = "one" }));

            var collection = new ThreadSafeToggleCollection();
            var serializer = new DynamicNewtonsoftJsonSerializer();
            serializer.TryLoad();

            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt", false);

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
            var callbackConfig = new EventCallbackConfig
            {
                TogglesUpdatedEvent = evt => { callbackEvent = evt; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._, false))
                .Returns(Task.FromResult(new FetchTogglesResult { HasChanged = false, ToggleCollection = new ToggleCollection(), Etag = "one" }));

            var collection = new ThreadSafeToggleCollection();
            var serializer = new DynamicNewtonsoftJsonSerializer();
            serializer.TryLoad();

            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, collection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt", false);

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            callbackEvent.Should().BeNull();
        }

        [Test]
        public void TogglesUpdated_Event_Is_Raised_After_ToggleCollection_Is_Updated()
        {
            // Arrange
            var fetchResultToggleCollection = new ToggleCollection();
            fetchResultToggleCollection.Features.Add(new FeatureToggle("toggle-1", "operational", true, false, new List<ActivationStrategy>())); // after toggles are fetched, the toggle is enabled

            var toggleCollection = new ThreadSafeToggleCollection();
            toggleCollection.Instance = new ToggleCollection();
            toggleCollection.Instance.Features.Add(new FeatureToggle("toggle-1", "operational", false, false, new List<ActivationStrategy>())); // initially, the toggle is NOT enabled

            var toggleIsEnabledResultAfterEvent = false;
            var callbackConfig = new EventCallbackConfig
            {
                // when toggles updated event is raised (after the fetch), check toggle collection to see if toggle is enabled
                TogglesUpdatedEvent = evt => { toggleIsEnabledResultAfterEvent = toggleCollection.Instance.Features.ElementAt(0).Enabled; }
            };

            var fakeApiClient = A.Fake<IUnleashApiClient>();
            A.CallTo(() => fakeApiClient.FetchToggles(A<string>._, A<CancellationToken>._, false))
                .Returns(Task.FromResult(new FetchTogglesResult { HasChanged = true, ToggleCollection = fetchResultToggleCollection, Etag = "one" }));

            var serializer = new DynamicNewtonsoftJsonSerializer();
            serializer.TryLoad();

            var filesystem = new MockFileSystem();
            var tokenSource = new CancellationTokenSource();
            var task = new FetchFeatureTogglesTask(fakeApiClient, toggleCollection, serializer, filesystem, callbackConfig, "togglefile.txt", "etagfile.txt", false);

            // Act
            Task.WaitAll(task.ExecuteAsync(tokenSource.Token));

            // Assert
            toggleCollection.Instance.Features.ElementAt(0).Enabled.Should().BeTrue(); // verify that toggle collection has been updated after fetch and shows that toggle is enabled
            toggleIsEnabledResultAfterEvent.Should().BeTrue(); // verify that toggles updated event handler got the correct result for the updated toggle state (should now be enabled)
        }
    }
}
