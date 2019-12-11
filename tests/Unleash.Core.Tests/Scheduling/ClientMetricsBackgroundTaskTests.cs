using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using Unleash.Communication;
using Unleash.Core.Tests.Utility;
using Unleash.Metrics;
using Unleash.Scheduling;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Core.Tests.Scheduling
{
    public class ClientMetricsBackgroundTaskTests
    {
        [Theory]
        [AutoMoqData]
        internal async Task ExecuteAsync_WhenApiReturns200Ok_CompletesSuccessfully(
            [Frozen] UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] Mock<IUnleashApiClientFactory> apiClientFactoryMock,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            [Frozen] MetricsBucket metricsBucket)
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            var threadSafeMetricsBucket = new ThreadSafeMetricsBucket(metricsBucket);
            var backgroundTask = new ClientMetricsBackgroundTask(apiClientFactoryMock.Object, settings, threadSafeMetricsBucket);

            httpMessageHandler.SetupPostSendMetricsRequestForSuccess(requestHeaders);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            apiClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(apiClient);

            await backgroundTask.ExecuteAsync(CancellationToken.None);

            httpMessageHandler.VerifyAll();
            apiClientFactoryMock.VerifyAll();
        }

        [Theory]
        [InlineAutoMoqData(HttpStatusCode.BadRequest, null, null)]
        [InlineAutoMoqData(HttpStatusCode.BadRequest, "Error", "text/plain")]
        [InlineAutoMoqData(HttpStatusCode.BadRequest, "{ \"Error\": true }", "application/json")]
        [InlineAutoMoqData(HttpStatusCode.InternalServerError, null, null)]
        [InlineAutoMoqData(HttpStatusCode.InternalServerError, "Error", "text/plain")]
        [InlineAutoMoqData(HttpStatusCode.InternalServerError, "{ \"Error\": true }", "application/json")]
        [InlineAutoMoqData(HttpStatusCode.ServiceUnavailable, null, null)]
        [InlineAutoMoqData(HttpStatusCode.ServiceUnavailable, "Error", "text/plain")]
        [InlineAutoMoqData(HttpStatusCode.ServiceUnavailable, "{ \"Error\": true }", "application/json")]
        internal async Task ExecuteAsync_WhenHttpExceptionOccurs_CompletesSuccessfully(
            HttpStatusCode statusCode,
            string responseBody,
            string responseContentType,
            [Frozen] UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] Mock<IUnleashApiClientFactory> apiClientFactoryMock,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            [Frozen] MetricsBucket metricsBucket)
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            var threadSafeMetricsBucket = new ThreadSafeMetricsBucket(metricsBucket);
            var backgroundTask = new ClientMetricsBackgroundTask(apiClientFactoryMock.Object, settings, threadSafeMetricsBucket);

            httpMessageHandler.SetupPostSendMetricsRequestForException(requestHeaders, statusCode, responseBody, responseContentType);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            apiClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(apiClient);

            await backgroundTask.ExecuteAsync(CancellationToken.None);

            httpMessageHandler.VerifyAll();
            apiClientFactoryMock.VerifyAll();
        }
    }
}
