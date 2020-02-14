using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unleash.Communication;
using FakeItEasy;
using System.Threading.Tasks;
using Unleash.ClientFactory;
using FluentAssertions;
using System;

namespace Unleash.Tests.ClientFactory
{
    public class SyncStartupUnitTest
    {
        private IUnleashApiClient mockApiClient { get; set; }
        private UnleashSettings settings { get; set; }
        private IUnleashClientFactory unleashFactory { get; set; }

        [SetUp]
        public void Setup()
        {
            mockApiClient = A.Fake<IUnleashApiClient>();
            settings = new MockedUnleashSettings();
            unleashFactory = new UnleashClientFactory();
        }

        [Test(Description = "Immediate initialization: Should only fetch toggles once")]
        public async Task ImmediateInitializationFetchCount()
        {
            settings.UnleashApiClient = mockApiClient;

            IUnleash unleash = await unleashFactory.Generate(settings, SynchronousInitialization: true);

            A.CallTo(() => mockApiClient.FetchToggles(string.Empty, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Test(Description = "Immediate initialization: Should be ready after creation")]
        public async Task ImmediateInitializationReadyAfterConstruction()
        {
            IUnleash unleash = await unleashFactory.Generate(settings, SynchronousInitialization: true);

            unleash.IsEnabled("one-enabled", false)
                .Should().BeTrue();
        }

        [Test(Description = "Immediate initialization: Should bubble up errors")]
        public void ImmediateInitializationBubbleErrors()
        {
            settings.UnleashApiClient = mockApiClient;
            A.CallTo(() => mockApiClient.FetchToggles(A<string>.Ignored, A<CancellationToken>.Ignored))
                .Throws<Exception>();

            Assert.ThrowsAsync<Exception>(async () => await unleashFactory.Generate(settings, SynchronousInitialization: true));
        }

        [Test(Description = "Immediate initialization: Should bubble up async fetch errors")]
        public void ImmediateInitializationBubbleAsyncErrors()
        {
            settings.UnleashApiClient = mockApiClient;
            A.CallTo(() => mockApiClient.FetchToggles(A<string>.Ignored, A<CancellationToken>.Ignored))
                .ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () => await unleashFactory.Generate(settings, SynchronousInitialization: true));
        }

        [Test(Description = "Delayed initialization: Should only fetch toggles once")]
        public async Task DelayedInitializationFetchCount()
        {
            settings.UnleashApiClient = mockApiClient;

            IUnleash unleash = await unleashFactory.Generate(settings);

            A.CallTo(() => mockApiClient.FetchToggles(string.Empty, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
