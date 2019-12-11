using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Unleash.Serialization;

namespace Unleash.Communication.Admin
{
    public abstract class BaseAdminApiClient
    {
        private HttpClient HttpClient { get; }
        private IJsonSerializer JsonSerializer { get; }

        protected BaseAdminApiClient(HttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            HttpClient = httpClient;
            JsonSerializer = jsonSerializer;
        }

        protected async Task<TResult> GetAsync<TResult>(string path, NameValueCollection queryString, CancellationToken cancellationToken)
        {
            var uri = new Uri(HttpClient.BaseAddress, path);
            var ub = new UriBuilder(uri) { Query = ToQueryString(queryString) };

            var responseMessage = await HttpClient.GetAsync(ub.Uri, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            using (var stream = await responseMessage.Content.ReadAsStreamAsync())
            {
                var result = JsonSerializer.Deserialize<TResult>(stream);
                return result;
            }
        }

        protected async Task<TResult> GetAsync<TResult>(string path, CancellationToken cancellationToken)
        {
            var responseMessage = await HttpClient.GetAsync(path, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            using (var stream = await responseMessage.Content.ReadAsStreamAsync())
            {
                var result = JsonSerializer.Deserialize<TResult>(stream);
                return result;
            }
        }

        protected async Task PostAsync(string path, CancellationToken cancellationToken)
        {
            var responseMessage = await HttpClient.PostAsync(path, null, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
        }

        protected async Task PostAsync<TRequest>(string path, TRequest request, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                JsonSerializer.Serialize(memoryStream, request);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var content = new StreamContent(memoryStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await HttpClient.PostAsync(path, content, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
            }
        }

        protected async Task<TResult> PostAsync<TRequest, TResult>(string path, TRequest request, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                JsonSerializer.Serialize(memoryStream, request);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var content = new StreamContent(memoryStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await HttpClient.PostAsync(path, content, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();

                using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                {
                    var result = JsonSerializer.Deserialize<TResult>(stream);
                    return result;
                }
            }
        }

        protected async Task PutAsync<TRequest>(string path, TRequest request, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                JsonSerializer.Serialize(memoryStream, request);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var content = new StreamContent(memoryStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await HttpClient.PutAsync(path, content, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
            }
        }

        protected async Task<TResult> PutAsync<TRequest, TResult>(string path, TRequest request, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                JsonSerializer.Serialize(memoryStream, request);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var content = new StreamContent(memoryStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await HttpClient.PutAsync(path, content, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();

                using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                {
                    var result = JsonSerializer.Deserialize<TResult>(stream);
                    return result;
                }
            }
        }

        protected async Task DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var responseMessage = await HttpClient.DeleteAsync(path, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
        }

        protected string ToQueryString(NameValueCollection parameters)
        {
            var encodingNameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            encodingNameValueCollection.Add(parameters);
            return encodingNameValueCollection.ToString();
        }
    }
}
