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

    protected static HttpResponseMessage Ok => 
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("")
        };

    protected static HttpResponseMessage NotModified => 
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotModified,
            Content = new StringContent("{}")
        };

    protected static HttpResponseMessage TooManyRequests => 
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.TooManyRequests,
            Content = new StringContent("")
        };
    
    protected static HttpResponseMessage InternalServerError =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("")
        };

    protected static HttpResponseMessage BadGateway =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadGateway,
            Content = new StringContent("")
        };

    protected static HttpResponseMessage ServiceUnavailable =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Content = new StringContent("")
        };

    protected static HttpResponseMessage Forbidden =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Forbidden,
            Content = new StringContent("")
        };

    protected static HttpResponseMessage Unauthorized =>
        new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("")
        };
}