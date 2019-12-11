using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Moq;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Core.Tests.Utility
{
    public static class MockFakeHttpMessageHandlerExtensions
    {
        internal static void SetupGetFeaturesRequestForSuccess(this Mock<FakeHttpMessageHandler> mock,
            IJsonSerializer jsonSerializer, ToggleCollection toggleCollectionResult,
            UnleashApiClientRequestHeaders requestHeaders)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Get, "api/client/features", requestHeaders))))
                .Returns(() => SerializedMessageResponse(jsonSerializer, toggleCollectionResult));
        }

        internal static void SetupGetFeaturesRequestForCacheHit(this Mock<FakeHttpMessageHandler> mock,
            UnleashApiClientRequestHeaders requestHeaders, string etag)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Get, "api/client/features", requestHeaders))))
                .Returns(() => CacheHitResponse(etag));
        }

        internal static void SetupGetFeaturesRequestForException(this Mock<FakeHttpMessageHandler> mock,
            UnleashApiClientRequestHeaders requestHeaders, HttpStatusCode responseStatusCode, string responseBody = null, string responseContentType = null)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Get, "api/client/features", requestHeaders))))
                .Returns(() => ExceptionResponse(responseStatusCode, responseBody, responseContentType));
        }

        internal static void SetupPostRegisterClientRequestForSuccess(this Mock<FakeHttpMessageHandler> mock, UnleashApiClientRequestHeaders requestHeaders)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Post, "api/client/register", requestHeaders))))
                .Returns(() => EmptyResponse(HttpStatusCode.OK));
        }

        internal static void SetupPostRegisterClientRequestForException(this Mock<FakeHttpMessageHandler> mock,
            UnleashApiClientRequestHeaders requestHeaders, HttpStatusCode statusCode, string responseBody = null, string responseContentType = null)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Post, "api/client/register", requestHeaders))))
                .Returns(() => ExceptionResponse(statusCode, responseBody, responseContentType));
        }

        internal static void SetupPostSendMetricsRequestForSuccess(this Mock<FakeHttpMessageHandler> mock, UnleashApiClientRequestHeaders requestHeaders)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Post, "api/client/metrics", requestHeaders))))
                .Returns(() => EmptyResponse(HttpStatusCode.OK));
        }

        internal static void SetupPostSendMetricsRequestForException(this Mock<FakeHttpMessageHandler> mock,
            UnleashApiClientRequestHeaders requestHeaders, HttpStatusCode statusCode, string responseBody = null, string responseContentType = null)
        {
            mock
                .Setup(mh => mh.Send(It.Is(ApiRequest(HttpMethod.Post, "api/client/metrics", requestHeaders))))
                .Returns(() => ExceptionResponse(statusCode, responseBody, responseContentType));
        }

        private static HttpResponseMessage EmptyResponse(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Version = new Version(1, 1),
            };

            return response;
        }

        private static HttpResponseMessage CacheHitResponse(string expectedEtag)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotModified,
                Version = new Version(1, 1),
            };

            response.Headers.ETag = new EntityTagHeaderValue(expectedEtag);

            return response;
        }

        private static HttpResponseMessage SerializedMessageResponse(IJsonSerializer jsonSerializer, ToggleCollection toggleCollectionResult)
        {
            var (payload, etag) = SerializePayloadAndCreateEtag(jsonSerializer, toggleCollectionResult);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Version = new Version(1, 1),
                Content = new ByteArrayContent(payload)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response.Headers.ETag = new EntityTagHeaderValue($"\"{etag}\"");

            return response;
        }

        private static HttpResponseMessage ExceptionResponse(HttpStatusCode statusCode, string responseBody = null, string responseContentType = null)
        {
            if ((int) statusCode < 400)
            {
                throw new ArgumentException($"The {nameof(statusCode)} supplied must be an error code", nameof(statusCode));
            }

            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Version = new Version(1, 1)
            };

            if (responseBody != null && responseContentType != null)
            {
                response.Content = new StringContent(responseBody);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(responseContentType);
            }

            return response;
        }

        private static (byte[] payload, string etag) SerializePayloadAndCreateEtag<T>(IJsonSerializer jsonSerializer, T entity)
        {
            var ms = new MemoryStream();
            jsonSerializer.Serialize(ms, entity);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] payload;
            string etag;
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(ms);
                etag = string.Join(string.Empty, bytes.Select(x => x.ToString("x2")));
                ms.Seek(0, SeekOrigin.Begin);
                payload = ms.ToArray();
            }

            return (payload, etag);
        }

        private static Expression<Func<HttpRequestMessage, bool>> ApiRequest(HttpMethod method, string path, UnleashApiClientRequestHeaders requestHeaders)
        {
            return message => message.RequestUri.LocalPath.Equals($"/{path}")
                              && message.Method.Equals(method)
                              && message.Headers.Any(x =>
                                  x.Key.Equals(UnleashApiClient.AppNameHeader) &&
                                  x.Value.Contains(requestHeaders.AppName, StringComparer.Ordinal))
                              && message.Headers.Any(x =>
                                  x.Key.Equals(UnleashApiClient.InstanceIdHeader) &&
                                  x.Value.Contains(requestHeaders.InstanceTag, StringComparer.Ordinal))
                              && (requestHeaders.CustomHttpHeaders == null
                                  || requestHeaders.CustomHttpHeaders.All(x =>
                                      message.Headers.Any(y =>
                                          y.Key.Equals(x.Key) && y.Value.Contains(x.Value, StringComparer.Ordinal))));
        }

        private static Expression<Func<HttpRequestMessage, bool>> ApiRequest<T>(HttpMethod method, string path, UnleashApiClientRequestHeaders requestHeaders, IJsonSerializer serializer, T entity)
        {
            var (payload, _) = SerializePayloadAndCreateEtag(serializer, entity);

            return message => message.RequestUri.LocalPath.Equals($"/{path}")
                                    && message.Method.Equals(method)
                                    && message.Headers.Any(x =>
                                        x.Key.Equals(UnleashApiClient.AppNameHeader) &&
                                        x.Value.Contains(requestHeaders.AppName, StringComparer.Ordinal))
                                    && message.Headers.Any(x =>
                                        x.Key.Equals(UnleashApiClient.InstanceIdHeader) &&
                                        x.Value.Contains(requestHeaders.InstanceTag, StringComparer.Ordinal))
                                    && (requestHeaders.CustomHttpHeaders == null
                                        || requestHeaders.CustomHttpHeaders.All(x =>
                                            message.Headers.Any(y =>
                                                y.Key.Equals(x.Key) &&
                                                y.Value.Contains(x.Value, StringComparer.Ordinal))))
                                    && message.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult().SequenceEqual(payload);
        }
    }
}
