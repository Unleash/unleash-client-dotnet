using System.Net;
using FakeItEasy;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;

public class BaseBackoffTest
{
    internal class CountingConfigurableHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<HttpResponseMessage> responses;
        public int CallCount { get; set; }

        public CountingConfigurableHttpMessageHandler(List<HttpResponseMessage> responses)
        {
            this.responses = responses;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = responses[CallCount];
            CallCount++;
            return response;
        }
    }

    internal UnleashApiClient GetClient(HttpMessageHandler messageHandler)
    {
        var httpClient = new HttpClient(messageHandler);
        httpClient.BaseAddress = new Uri("http://localhost:8080/");

        var apiClient = new UnleashApiClient(
            httpClient,
            A.Fake<IJsonSerializer>(),
            A.Fake<UnleashApiClientRequestHeaders>(),
            new EventCallbackConfig()
        );
        return apiClient;
    }

    /// <summary>
    /// 200 OK
    /// </summary>
    protected static HttpResponseMessage Ok =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("")
        };

    /// <summary>
    /// 304 Not Modified
    /// </summary>
    protected static HttpResponseMessage NotModified =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotModified,
            Content = new StringContent("{}")
        };

    /// <summary>
    /// 401 Unauthorized
    /// </summary>
    protected static HttpResponseMessage Unauthorized =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("")
        };

    /// <summary>
    /// 403 Forbidden
    /// </summary>
    protected static HttpResponseMessage Forbidden =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Forbidden,
            Content = new StringContent("")
        };

    /// <summary>
    /// 404 Not Found
    /// </summary>
    protected static HttpResponseMessage NotFound =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("")
        };

    /// <summary>
    /// 429 Too Many Requests
    /// </summary>
    protected static HttpResponseMessage TooManyRequests =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.TooManyRequests,
            Content = new StringContent("")
        };

    /// <summary>
    /// 500 Internal Server Error
    /// </summary>
    protected static HttpResponseMessage InternalServerError =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("")
        };

    /// <summary>
    /// 502 Bad Gateway
    /// </summary>
    protected static HttpResponseMessage BadGateway =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadGateway,
            Content = new StringContent("")
        };

    /// <summary>
    /// 503 Service Unavailable
    /// </summary>
    protected static HttpResponseMessage ServiceUnavailable =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Content = new StringContent("")
        };

    /// <summary>
    /// 504 Gateway Timeout
    /// </summary>
    protected static HttpResponseMessage GatewayTimeout =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.GatewayTimeout,
            Content = new StringContent("")
        };
}