using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Logging;
using Unleash.Util;

namespace Unleash.Repository
{
    internal class FetchFeatureTogglesTask : IBackgroundTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));

        internal const string FeaturesUri = "/api/client/features";

        // In-memory representation of the etag of a given request.
        internal string Etag { get; private set; }

        private readonly UnleashConfig config;
        private readonly ToggleCollectionInstance toggleCollectionInstance;

        public FetchFeatureTogglesTask(
            UnleashConfig config,
            ToggleCollectionInstance toggleCollectionInstance)
        {
            this.config = config;
            this.toggleCollectionInstance = toggleCollectionInstance;

            Etag = config.Services.FileSystem.ReadAllText(config.BackupFile);
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, FeaturesUri))
            {
                request.SetRequestProperties(config);

                if (EntityTagHeaderValue.TryParse(Etag, out var etagHeaderValue))
                    request.Headers.IfNoneMatch.Add(etagHeaderValue);

                using (var response = await config.Services.HttpClient.SendAsync(request, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in '{nameof(FetchFeatureTogglesTask)}': " + error);

                        return;
                    }

                    if (response.Headers.ETag?.Tag == Etag)
                        return;
                    
                    using (var fileStream = File.Open(config.BackupFile, FileMode.Create))
                        await response.Content.CopyToAsync(fileStream);

                    Etag = response.Headers.ETag?.Tag;
                    config.Services.FileSystem.WriteAllText(config.BackupEtagFile, Etag);

                    using (var fileStream = config.Services.FileSystem.FileOpenRead(config.BackupFile))
                    {
                        var toggleCollection = config.Services.JsonSerializer.Deserialize<ToggleCollection>(fileStream);
                        toggleCollectionInstance.Update(toggleCollection);
                    }
                }
            }
        }
    }
}