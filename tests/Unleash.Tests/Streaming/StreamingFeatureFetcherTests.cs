using Unleash;
using Unleash.Streaming;
using Unleash.Communication;
using NUnit.Framework;
using Unleash.Metrics;
using Yggdrasil;
using Unleash.Scheduling;
using LaunchDarkly.EventSource;


internal class MockedTaskManager : Unleash.Scheduling.IUnleashScheduledTaskManager
{
    public void Configure(IEnumerable<IUnleashScheduledTask> tasks, CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
        // Mock dispose method
    }
}

internal class StubbedApiClient : IUnleashApiClient
{
    public IStreamingEventHandler StreamingEventHandler { get; private set; }

    public Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken, bool throwOnFail = false)
    {
        return Task.FromResult(new FetchTogglesResult());
    }

    public Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SendMetrics(MetricsBucket metrics, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task StartStreamingAsync(Uri apiUri, IStreamingEventHandler streamingEventHandler)
    {
        StreamingEventHandler = streamingEventHandler;
        return Task.CompletedTask;
    }

    public void StopStreaming()
    {
    }
}

public class StreamingFeatureFetcherTests
{
    [Test]
    public async Task Handles_Messages()
    {
        // Arrange
        var apiClient = new StubbedApiClient();
        var uri = new Uri("http://example.com/streaming");
        var settings = new UnleashSettings
        {
            UnleashApiClient = apiClient,
            AppName = "TestApp",
            InstanceTag = "TestInstance",
            ScheduledTaskManager = new MockedTaskManager()
        };
        settings.Experimental.UseStreaming(uri);
        var unleash = new DefaultUnleash(settings);
        var payload = "{\"events\":[{\"type\":\"hydration\",\"eventId\":1,\"features\":[{\"name\":\"deltaFeature\",\"enabled\":true,\"strategies\":[],\"variants\":[]}],\"segments\":[]}]}";

        // Act
        apiClient.StreamingEventHandler.HandleMessage(null, new MessageReceivedEventArgs(new MessageEvent("unleash-connected", payload, uri)));
        var enabled = unleash.IsEnabled("deltaFeature");

        // Assert
        Assert.IsTrue(enabled, "Feature should be enabled after handling the message.");
    }
}