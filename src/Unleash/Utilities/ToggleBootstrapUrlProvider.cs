using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Serialization;

namespace Unleash.Utilities
{
    public class ToggleBootstrapUrlProvider : IToggleBootstrapProvider
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ToggleBootstrapUrlProvider));

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient client;
        private readonly IJsonSerializer jsonSerializer;
        private readonly string path;
        private readonly bool throwOnFail;
        private readonly Dictionary<string, string> customHeaders;

        public ToggleBootstrapUrlProvider(string path, HttpClient client, IJsonSerializer jsonSerializer, bool throwOnFail = false, Dictionary<string, string> customHeaders = null)
        {
            this.path = path;
            this.client = client;
            this.jsonSerializer = jsonSerializer;
            this.throwOnFail = throwOnFail;
            this.customHeaders = customHeaders;
        }

        public ToggleCollection Read()
        {
            return Task.Run(() => FetchFile()).GetAwaiter().GetResult();
        }

        private async Task<ToggleCollection> FetchFile()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, path))
            {
                if (customHeaders != null)
                {
                    foreach (var keyValuePair in customHeaders)
                    {
                        request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                using (var response = await client.SendAsync(request, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Logger.Trace($"UNLEASH: Error {response.StatusCode} from server in 'ToggleBootstrapUrlProvider.{nameof(FetchFile)}': " + error);

                        if (throwOnFail)
                            throw new FetchingToggleBootstrapUrlFailedException("Failed to fetch feature toggles", response.StatusCode);

                        return null;
                    }

                    try
                    {
                        var togglesResponseStream = await response.Content.ReadAsStreamAsync();
                        return jsonSerializer.Deserialize<ToggleCollection>(togglesResponseStream);
                    }
                    catch (Exception ex)
                    {
                        Logger.Trace($"UNLEASH: Exception in 'ToggleBootstrapUrlProvider.{nameof(FetchFile)}' during reading and deserializing ToggleCollection from stream: " + ex.Message);

                        if (throwOnFail)
                            throw new UnleashException("Exception during reading and deserializing ToggleCollection from stream", ex);

                        return null;
                    }
                }
            }
        }
    }
}
