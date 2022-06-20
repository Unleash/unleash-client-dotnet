using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Scheduling
{
    internal class FetchFeatureTogglesTask : IUnleashScheduledTask
    {
        private readonly string toggleFile;
        private readonly string etagFile;

        private readonly IFileSystem fileSystem;
        private readonly IUnleashApiClient apiClient;
        private readonly IJsonSerializer jsonSerializer;
        private readonly ThreadSafeToggleCollection toggleCollection;

        // In-memory reference of toggles/etags
        internal string Etag { get; set; }

        public FetchFeatureTogglesTask(
            IUnleashApiClient apiClient,
            ThreadSafeToggleCollection toggleCollection,
            IJsonSerializer jsonSerializer,
            IFileSystem fileSystem,
            string toggleFile,
            string etagFile)
        {
            this.apiClient = apiClient;
            this.toggleCollection = toggleCollection;
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleFile = toggleFile;
            this.etagFile = etagFile;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var result = await apiClient.FetchToggles(Etag, cancellationToken).ConfigureAwait(false);

            if (!result.HasChanged)
                return;

            if (string.IsNullOrEmpty(result.Etag))
                return;

            if (result.Etag == Etag)
                return;

            toggleCollection.Instance = result.ToggleCollection;
            Console.WriteLine("My toggle collection is now", result.ToggleCollection);

            using (var fs = fileSystem.FileOpenCreate(toggleFile))
            {
                Console.WriteLine("Writing toggles to" + toggleFile);
                jsonSerializer.Serialize(fs, result.ToggleCollection);
            }

            Etag = result.Etag;
            fileSystem.WriteAllText(etagFile, Etag);
        }

        public string Name => "fetch-feature-toggles-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}