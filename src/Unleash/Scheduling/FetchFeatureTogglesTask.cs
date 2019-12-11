using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Caching;
using Unleash.Communication;
using Unleash.Internal;

namespace Unleash.Scheduling
{
    internal class FetchFeatureTogglesTask : IUnleashScheduledTask
    {
        private bool firstSuccessfulRun = true;
        private readonly IUnleashApiClientFactory apiClientFactory;
        private readonly ThreadSafeToggleCollection toggleCollection;
        private readonly IToggleCollectionCache toggleCollectionCache;
        private readonly TaskCompletionSource<object> initializationTaskCompletionSource;

        // In-memory reference of toggles/etags
        internal string Etag { get; set; }

        public FetchFeatureTogglesTask(
            IUnleashApiClientFactory apiClientFactory,
            ThreadSafeToggleCollection toggleCollection,
            IToggleCollectionCache toggleCollectionCache,
            TaskCompletionSource<object> initializationTaskCompletionSource)
        {
            this.apiClientFactory = apiClientFactory;
            this.toggleCollection = toggleCollection;
            this.toggleCollectionCache = toggleCollectionCache;
            this.initializationTaskCompletionSource = initializationTaskCompletionSource;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var apiClient = apiClientFactory.CreateClient();
            FetchTogglesResult result = null;

            try
            {
                result = await apiClient.FetchToggles(Etag, cancellationToken).ConfigureAwait(false);

                if (firstSuccessfulRun)
                {
                    firstSuccessfulRun = false;
                    initializationTaskCompletionSource.SetResult(null);
                }

                if (!result.HasChanged)
                    return;

                if (string.IsNullOrEmpty(result.Etag))
                    return;

                if (result.Etag == Etag)
                    return;

                toggleCollection.Instance = result.ToggleCollection;

                await toggleCollectionCache.Save(result.ToggleCollection, result.Etag, cancellationToken).ConfigureAwait(false);

                Etag = result.Etag;
            }
            catch (OperationCanceledException)
            {
                initializationTaskCompletionSource.SetCanceled();
            }
        }

        public string Name => "fetch-feature-toggles-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}
