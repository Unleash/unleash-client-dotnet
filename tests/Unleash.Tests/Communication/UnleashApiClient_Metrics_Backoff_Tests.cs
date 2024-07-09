using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Serialization;

public class UnleashApiClient_Metrics_Backoff_Tests : BaseBackoffTest
{
    [Test]
    public void Should_Not_Skip_First_Call()
    {
        // Arrange
        var messageHandler = new CountingConfigurableHttpMessageHandler
        (
            new List<HttpResponseMessage>
            {
                Ok
            }
        );
        var httpClient = new HttpClient(messageHandler);
        httpClient.BaseAddress = new Uri("http://localhost:8080/");

        var apiClient = new UnleashApiClient(
            httpClient,
            A.Fake<IJsonSerializer>(),
            A.Fake<UnleashApiClientRequestHeaders>(),
            new EventCallbackConfig()
        );

        // Act
        var result = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert
        messageHandler.CallCount.Should().Be(1);
    }

    [Test]
    public void NotModified_Should_Not_Skip_Next()
    {
        // Arrange
        var messageHandler = new CountingConfigurableHttpMessageHandler
        (
            new List<HttpResponseMessage>
            {
                NotModified,
                Ok
            }
        );
        var httpClient = new HttpClient(messageHandler);
        httpClient.BaseAddress = new Uri("http://localhost:8080/");

        var apiClient = new UnleashApiClient(
            httpClient,
            A.Fake<IJsonSerializer>(),
            A.Fake<UnleashApiClientRequestHeaders>(),
            new EventCallbackConfig()
        );

        // Act
        var result = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();
        var result2 = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert
        messageHandler.CallCount.Should().Be(2);
    }

    [Test]
    public void Backoff_Caps_Out_At_10()
    {
        // Arrange
        var messageHandler = new CountingConfigurableHttpMessageHandler
        (
            new List<HttpResponseMessage>
            {
                TooManyRequests,
                TooManyRequests,
                InternalServerError,
                TooManyRequests,
                TooManyRequests,
                InternalServerError,
                InternalServerError,
                GatewayTimeout,
                BadGateway,
                ServiceUnavailable,
                Forbidden,
                Unauthorized,
                Ok
            }
        );
        var httpClient = new HttpClient(messageHandler);
        httpClient.BaseAddress = new Uri("http://localhost:8080/");

        var apiClient = new UnleashApiClient(
            httpClient,
            A.Fake<IJsonSerializer>(),
            A.Fake<UnleashApiClientRequestHeaders>(),
            new EventCallbackConfig()
        );

        // Act
        var result = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert
        messageHandler.CallCount.Should().Be(1);
    }

    [Test]
    public void Should_Skip_One_After_First_429()
    {
        // Arrange
        var messageHandler = new CountingConfigurableHttpMessageHandler
        (
            new List<HttpResponseMessage>
            {
                TooManyRequests,
                Ok
            }
        );
        var apiClient = GetClient(messageHandler);

        // Act 1
        var result = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert 1
        messageHandler.CallCount.Should().Be(1);

        // Act 2
        var result2 = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();
        var result3 = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert 2
        messageHandler.CallCount.Should().Be(2);
    }

    [Test]
    public void Access_Error_Responses_Goes_Straight_To_Ten()
    {
        // Arrange
        var messageHandler = new CountingConfigurableHttpMessageHandler
        (
            new List<HttpResponseMessage>
            {
                Unauthorized,
                Ok
            }
        );
        var apiClient = GetClient(messageHandler);

        // Act 1
        var result = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert 1
        messageHandler.CallCount.Should().Be(1);

        // Act 2
        for (var i = 0; i < 10; i++)
        {
            var innerResult = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
                .GetAwaiter()
                .GetResult();
        }

        messageHandler.CallCount.Should().Be(1);

        // This the 11th call should be allowed through and cause an increase in CallCount
        var result2 = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        // Assert 2
        messageHandler.CallCount.Should().Be(2);
    }

    [Test]
    public void Backoff_Gradually_Decreases()
    {
        // Arrange
        var messageHandler = new CountingConfigurableHttpMessageHandler
        (
            new List<HttpResponseMessage>
            {
                Unauthorized,
                Ok,
                Ok,
                Ok,
                Ok
            }
        );
        var apiClient = GetClient(messageHandler);

        // Unauthorized = 10 skips
        var result = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();


        // 11 attempts = 1 additional call attempt - which returns 200
        for (var i = 0; i < 11; i++)
        {
            var innerResult = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
                .GetAwaiter()
                .GetResult();
        }

        messageHandler.CallCount.Should().Be(2);

        // 10 attempts = 1 additional call attempt (should have decreased to 9 skips) - returns 200
        for (var i = 0; i < 10; i++)
        {
            var innerResult = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
                .GetAwaiter()
                .GetResult();
        }

        messageHandler.CallCount.Should().Be(3);

        // Should have decreased to 8 skips, so making 8 tries here so we can verify
        for (var i = 0; i < 8; i++)
        {
            var innerResult = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
                .GetAwaiter()
                .GetResult();
        }

        messageHandler.CallCount.Should().Be(3);

        // Make one more attempt - should result in an attempt
        var result2 = Task.Run(() => apiClient.FetchToggles("etag", CancellationToken.None))
            .GetAwaiter()
            .GetResult();

        messageHandler.CallCount.Should().Be(4);
    }
}