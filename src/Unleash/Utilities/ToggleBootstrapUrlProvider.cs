using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;

namespace Unleash.Utilities
{
    public class ToggleBootstrapUrlProvider : IToggleBootstrapProvider
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient client;
        private readonly string path;
        private readonly bool throwOnFail;

        public ToggleBootstrapUrlProvider(string path, bool throwOnFail = false)
        {
            this.path = path;
            client = new HttpClient();
            this.throwOnFail = throwOnFail;
        }

        public ToggleBootstrapUrlProvider(string path, HttpClient client, bool throwOnFail = false)
        {
            this.path = path;
            this.client = client;
            this.throwOnFail = throwOnFail;
        }

        public string Read()
        {
            return Task.Run(() => FetchFile()).Result;
        }

        private async Task<string> FetchFile()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, path))
            {
                using (var response = await client.SendAsync(request, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        if (throwOnFail)
                            throw new FetchingToggleBootstrapUrlFailedException("Failed to fetch feature toggles", response.StatusCode);

                        return string.Empty;
                    }

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
