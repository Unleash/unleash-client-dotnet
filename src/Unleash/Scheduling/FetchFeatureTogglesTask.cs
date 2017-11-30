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
        private readonly ToggleCollectionInstance toggleCollectionInstance;
        private readonly IJsonSerializer jsonSerializer;

        // In-memory representation of the etag of a given request.
        internal string Etag { get; private set; }

        public FetchFeatureTogglesTask(
            IUnleashApiClient apiClient,
            ToggleCollectionInstance toggleCollectionInstance, 
            IJsonSerializer jsonSerializer,
            IFileSystem fileSystem, 
            string toggleFile, 
            string etagFile)
        {
            this.apiClient = apiClient;
            this.toggleCollectionInstance = toggleCollectionInstance;
            this.jsonSerializer = jsonSerializer;
            this.fileSystem = fileSystem;
            this.toggleFile = toggleFile;
            this.etagFile = etagFile;

            Etag = fileSystem.ReadAllText(this.etagFile);
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var result = await apiClient.FetchToggles(Etag, cancellationToken).ConfigureAwait(false);

            if (result.Etag == Etag)
                return;

            toggleCollectionInstance.Update(result.ToggleCollection);

            using (var stream = jsonSerializer.Serialize(result.ToggleCollection))
            using (var fileStream = File.Open(toggleFile, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream, 1024 * 4, cancellationToken).ConfigureAwait(false);
            }

            Etag = result.Etag;
            fileSystem.WriteAllText(etagFile, Etag);
        }

        public string Name => "fetch-feature-toggles-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }
    }
}