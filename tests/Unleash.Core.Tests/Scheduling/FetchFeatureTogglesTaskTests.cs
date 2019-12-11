using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using Unleash.Caching;
using Unleash.Communication;
using Unleash.Core.Tests.Utility;
using Unleash.Internal;
using Unleash.Scheduling;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Core.Tests.Scheduling
{
    public class FetchFeatureTogglesTaskTests
    {
        [Theory]
        [AutoMoqData]
        internal async Task ExecuteAsync_WhenApiReturns200Ok_CompletesSuccessfully(
            [Frozen] UnleashSettings settings,
            [Frozen] ThreadSafeToggleCollection toggleCollection,
            [Frozen] Mock<IToggleCollectionCache> toggleCollectionCache,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] Mock<IUnleashApiClientFactory> apiClientFactoryMock,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            TaskCompletionSource<object> taskCompletionSource)
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            var toggleCollectionInstance = toggleCollection.Instance;

            toggleCollectionCache
                .Setup(
                    x => x.Save(
                        It.Is<ToggleCollection>(
                            y => y.Version.Equals(toggleCollectionInstance.Version)
                                && y.Features.All(
                                     z => toggleCollectionInstance.Features.Any(zz => zz.Name.Equals(z.Name))
                                )
                        ),
                        It.IsAny<string>(),
                        CancellationToken.None
                    )
                 )
                .Returns(Task.CompletedTask);

            var backgroundTask = new FetchFeatureTogglesTask(apiClientFactoryMock.Object, toggleCollection, toggleCollectionCache.Object, taskCompletionSource);

            httpMessageHandler.SetupGetFeaturesRequestForSuccess(jsonSerializer, toggleCollectionInstance, requestHeaders);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            apiClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(apiClient);

            await backgroundTask.ExecuteAsync(CancellationToken.None);

            Assert.True(taskCompletionSource.Task.IsCompletedSuccessfully);

            httpMessageHandler.VerifyAll();
            apiClientFactoryMock.VerifyAll();
            toggleCollectionCache.VerifyAll();
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
            [Frozen] ThreadSafeToggleCollection toggleCollection,
            [Frozen] Mock<IToggleCollectionCache> toggleCollectionCache,
            [Frozen] Mock<FakeHttpMessageHandler> httpMessageHandler,
            [Frozen] Mock<IUnleashApiClientFactory> apiClientFactoryMock,
            [Frozen] UnleashApiClientRequestHeaders requestHeaders,
            TaskCompletionSource<object> taskCompletionSource)
        {
            var jsonSerializerSettings = new NewtonsoftJsonSerializerSettings();
            var jsonSerializer = new NewtonsoftJsonSerializer(jsonSerializerSettings);

            var backgroundTask = new FetchFeatureTogglesTask(apiClientFactoryMock.Object, toggleCollection, toggleCollectionCache.Object, taskCompletionSource);

            httpMessageHandler.SetupGetFeaturesRequestForException(requestHeaders, statusCode, responseBody, responseContentType);

            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = settings.UnleashApi };
            var apiClient = new UnleashApiClient(httpClient, jsonSerializer, requestHeaders);
            apiClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(apiClient);

            await backgroundTask.ExecuteAsync(CancellationToken.None);

            Assert.True(taskCompletionSource.Task.IsCompletedSuccessfully);

            httpMessageHandler.VerifyAll();
            apiClientFactoryMock.VerifyAll();
            toggleCollectionCache.VerifyNoOtherCalls();
        }
    }
}
