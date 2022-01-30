using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Utilities
{
    public class ToggleBootstrapUrlProvider : IToggleBootstrapProvider
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient client;
        private readonly IJsonSerializer jsonSerializer;
        private readonly string path;
        private readonly bool throwOnFail;

        public ToggleBootstrapUrlProvider(string path, HttpClient client, IJsonSerializer jsonSerializer, bool throwOnFail = false)
        {
            this.path = path;
            this.client = client;
            this.jsonSerializer = jsonSerializer;
            this.throwOnFail = throwOnFail;
        }

        public ToggleCollection Read()
        {
            return Task.Run(() => FetchFile()).GetAwaiter().GetResult();
        }

        private async Task<ToggleCollection> FetchFile()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, path))
            {
                using (var response = await client.SendAsync(request, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        if (throwOnFail)
                            throw new FetchingToggleBootstrapUrlFailedException("Failed to fetch feature toggles", response.StatusCode);

                        return null;
                    }

                    var togglesResponseStream = await response.Content.ReadAsStreamAsync();
                    return jsonSerializer.Deserialize<ToggleCollection>(togglesResponseStream);
                }
            }
        }
    }
}
