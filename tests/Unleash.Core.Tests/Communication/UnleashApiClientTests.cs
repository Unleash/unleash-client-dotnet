using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using Unleash.Communication;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Unleash.Metrics;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Core.Tests.Communication
{
    public class UnleashApiClientTests
    {
        [Theory]
        [AutoMoqData]
        internal async Task FetchToggles_WhenServerReturnsExpectedToggles_ReturnsValidResult(
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            string etag
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            var expectedToggles = new ToggleCollection();

            httpMessageHandler.SetupGetFeaturesRequestForSuccess(jsonSerializer, expectedToggles, requestHeaders);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var actualFetchTogglesResult = await apiClient.FetchToggles(etag, CancellationToken.None);
            var actualToggles = actualFetchTogglesResult.ToggleCollection;

            Assert.Equal(expectedToggles.Features, actualToggles.Features);
            Assert.True(actualFetchTogglesResult.HasChanged);
            Assert.NotEqual(etag, actualFetchTogglesResult.Etag);

            httpMessageHandler.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        internal async Task FetchToggles_WhenServerReturns304NotModified_ReturnsCacheHitResult(
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            string etagMinusQuotes
        )
        {
            var etag = $"\"{etagMinusQuotes}\"";

            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            httpMessageHandler.SetupGetFeaturesRequestForCacheHit(requestHeaders, etag);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var actualFetchTogglesResult = await apiClient.FetchToggles(etag, CancellationToken.None);

            Assert.Null(actualFetchTogglesResult.ToggleCollection);
            Assert.False(actualFetchTogglesResult.HasChanged);
            Assert.Equal(etag, actualFetchTogglesResult.Etag);

            httpMessageHandler.VerifyAll();
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
        internal async Task FetchToggles_WhenHttpExceptionOccurs_ReturnsEmptyResult(
            HttpStatusCode responseStatusCode,
            string responseBody,
            string responseContentType,
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            string etag
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            httpMessageHandler.SetupGetFeaturesRequestForException(requestHeaders, responseStatusCode, responseBody, responseContentType);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var actualFetchTogglesResult = await apiClient.FetchToggles(etag, CancellationToken.None);

            Assert.Null(actualFetchTogglesResult.ToggleCollection);
            Assert.Null(actualFetchTogglesResult.Etag);
            Assert.False(actualFetchTogglesResult.HasChanged);

            httpMessageHandler.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        internal async Task RegisterClient_WhenServerReturns200Ok_ReturnsTrue(
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            ClientRegistration clientRegistration
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            httpMessageHandler.SetupPostRegisterClientRequestForSuccess(requestHeaders);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var successResult = await apiClient.RegisterClient(clientRegistration, CancellationToken.None);

            Assert.True(successResult);

            httpMessageHandler.VerifyAll();
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
        internal async Task RegisterClient_WhenHttpExceptionOccurs_ReturnsFalse(
            HttpStatusCode responseStatusCode,
            string responseBody,
            string responseContentType,
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            ClientRegistration clientRegistration
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            httpMessageHandler.SetupPostRegisterClientRequestForException(requestHeaders, responseStatusCode, responseBody, responseContentType);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var successResult = await apiClient.RegisterClient(clientRegistration, CancellationToken.None);

            Assert.False(successResult);

            httpMessageHandler.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        internal async Task RegisterMetrics_WhenServerReturns200Ok_ReturnsTrue(
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            MetricsBucket metricsBucket
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            httpMessageHandler.SetupPostSendMetricsRequestForSuccess(requestHeaders);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var threadSafeMetricsBucket = new ThreadSafeMetricsBucket(metricsBucket);
            var successResult = await apiClient.SendMetrics(threadSafeMetricsBucket, CancellationToken.None);

            Assert.True(successResult);

            httpMessageHandler.VerifyAll();
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
        internal async Task RegisterMetrics_WhenHttpExceptionOccurs_ReturnsFalse(
            HttpStatusCode responseStatusCode,
            string responseBody,
            string responseContentType,
            UnleashSettings settings,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            MetricsBucket metricsBucket
        )
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            httpMessageHandler.SetupPostSendMetricsRequestForException(requestHeaders, responseStatusCode, responseBody, responseContentType);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);

            var threadSafeMetricsBucket = new ThreadSafeMetricsBucket(metricsBucket);
            var successResult = await apiClient.SendMetrics(threadSafeMetricsBucket, CancellationToken.None);

            Assert.False(successResult);

            httpMessageHandler.VerifyAll();
        }
    }
}
