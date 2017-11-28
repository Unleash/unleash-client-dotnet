using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Serialization;
using Unleash.Util;

namespace Unleash.Repository
{
    internal class FetchFeatureTogglesTask : IBackgroundTask
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

        public async Task Execute(CancellationToken cancellationToken)
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
    }

    //internal class FetchFeatureTogglesTask : IBackgroundTask
    //{
    //    private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));

    //    internal const string FeaturesUri = "/api/client/features";

    //    // In-memory representation of the etag of a given request.
    //    internal string Etag { get; private set; }

    //    private readonly UnleashConfig config;
    //    private readonly ToggleCollectionInstance toggleCollectionInstance;

    //    public FetchFeatureTogglesTask(
    //        UnleashConfig config,
    //        ToggleCollectionInstance toggleCollectionInstance)
    //    {
    //        this.config = config;
    //        this.toggleCollectionInstance = toggleCollectionInstance;

    //        Etag = config.Services.FileSystem.ReadAllText(config.BackupFile);
    //    }

    //    public async Task Execute(CancellationToken cancellationToken)
    //    {
    //        using (var request = new HttpRequestMessage(HttpMethod.Get, FeaturesUri))
    //        {
    //            request.SetRequestProperties(config);

    //            if (EntityTagHeaderValue.TryParse(Etag, out var etagHeaderValue))
    //                request.Headers.IfNoneMatch.Add(etagHeaderValue);

    //            using (var response = await config.Services.HttpClient.SendAsync(request, cancellationToken))
    //            {
    //                if (!response.IsSuccessStatusCode)
    //                {
    //                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    //                    Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(FetchFeatureTogglesTask)}': " + error);

    //                    return;
    //                }

    //                if (response.Headers.ETag?.Tag == Etag)
    //                    return;
                    
    //                using (var fileStream = File.Open(config.BackupFile, FileMode.Create))
    //                    await response.Content.CopyToAsync(fileStream);

    //                Etag = response.Headers.ETag?.Tag;
    //                config.Services.FileSystem.WriteAllText(config.BackupEtagFile, Etag);

    //                using (var fileStream = config.Services.FileSystem.FileOpenRead(config.BackupFile))
    //                {
    //                    var toggleCollection = config.Services.JsonSerializer.Deserialize<ToggleCollection>(fileStream);
    //                    toggleCollectionInstance.Update(toggleCollection);
    //                }
    //            }
    //        }
    //    }
    //}
}