using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Repository;

namespace Unleash.Util
{
    internal class FetchFeatureTogglesTask : IBackgroundTask
    {
        internal const string FeaturesUri = "/api/client/features";

        // In-memory representation of the etag of a given request.
        private string etag;

        private readonly UnleashConfig config;
        private readonly ToggleCollectionInstance toggleCollectionInstance;

        public FetchFeatureTogglesTask(
            UnleashConfig config,
            ToggleCollectionInstance toggleCollectionInstance)
        {
            this.config = config;
            this.toggleCollectionInstance = toggleCollectionInstance;

            etag = config.Services.FileSystem.ReadAllText(config.BackupFile);
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, FeaturesUri))
            {
                request.SetRequestProperties(config);

                if (EntityTagHeaderValue.TryParse(etag, out var etagHeaderValue))
                    request.Headers.IfNoneMatch.Add(etagHeaderValue);

                using (var result = await config.Services.HttpClient.SendAsync(request, cancellationToken))
                {
                    if (result.Headers.ETag?.Tag == etag)
                        return;
                    
                    using (var fileStream = File.Open(config.BackupFile, FileMode.Create))
                        await result.Content.CopyToAsync(fileStream);

                    etag = result.Headers.ETag?.Tag;
                    config.Services.FileSystem.WriteAllText(config.BackupEtagFile, etag);

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