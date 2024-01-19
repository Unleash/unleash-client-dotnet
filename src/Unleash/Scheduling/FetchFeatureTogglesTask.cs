using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Logging;

namespace Unleash.Scheduling
{
    internal class FetchFeatureTogglesTask : IUnleashScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));
        private readonly FetchFeatureToggles fetchFeatureToggles;
        private readonly Action<ToggleCollection, string> onFlagsUpdated;

        // In-memory reference of toggles/etags
        internal string Etag { get; set; }

        public FetchFeatureTogglesTask(
            FetchFeatureToggles fetchFeatureToggles,
            Action<ToggleCollection, string> OnFlagsUpdated)
        {
            this.fetchFeatureToggles = fetchFeatureToggles;
            onFlagsUpdated = OnFlagsUpdated;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            (ToggleCollection collection, string etag, bool hasChanged) = await fetchFeatureToggles.FetchToggles(cancellationToken);
            if (hasChanged)
            {
                onFlagsUpdated(collection, etag);
            }
        }

        public string Name => "fetch-feature-toggles-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
        public bool Enabled { get; set; }
    }
}