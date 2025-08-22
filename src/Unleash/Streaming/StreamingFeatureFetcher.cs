using Unleash.Communication;
using Yggdrasil;
using LaunchDarkly.EventSource;
using System;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Events;
using Unleash.Logging;

namespace Unleash.Streaming
{
    /// <summary>
    /// </summary>
    internal class StreamingFeatureFetcher : IStreamingFeatureFetcher, IStreamingEventHandler
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(StreamingFeatureFetcher));

        public StreamingFeatureFetcher(UnleashSettings settings, IUnleashApiClient apiClient, YggdrasilEngine engine, EventCallbackConfig eventConfig)
        {
            Settings = settings;
            ApiClient = apiClient;
            Engine = engine;
            EventConfig = eventConfig;
        }

        private YggdrasilEngine Engine { get; set; }
        private EventCallbackConfig EventConfig { get; set; }
        private UnleashSettings Settings { get; set; }
        private IUnleashApiClient ApiClient { get; set; }

        public async Task StartAsync()
        {
            try
            {
                await ApiClient.StartStreamingAsync(Settings.ExperimentalStreamingUri, this).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = ex });
                throw new UnleashException("Exception while starting streaming", ex);
            }
        }

        public void HandleMessage(object target, MessageReceivedEventArgs data)
        {
            switch (data.EventName)
            {
                case "unleash-connected":
                case "unleash-updated":
                    Logger.Debug(() => $"UNLEASH: Handling event '{data.EventName}'");
                    HandleStreamingUpdate(data.Message.Data);
                    break;
                default:
                    Logger.Debug(() => $"UNLEASH: Ignoring unknown event type: {data.EventName}");
                    break;
            }
        }

        public void HandleStreamingUpdate(string data)
        {
            try
            {
                Engine.TakeState(data);
                // now that the toggle collection has been updated, raise the toggles updated event if configured
                EventConfig?.RaiseTogglesUpdated(new TogglesUpdatedEvent { UpdatedOn = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Logger.Error(() => $"UNLEASH: Error processing streaming event, feature flags will likely not evaluate correctly until application restart or stream re-connect", ex);
                EventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = ex });
            }
        }

        public void HandleError(object target, ExceptionEventArgs data)
        {
            // Handle any errors that occur during streaming
            EventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = data.Exception });
        }

        public void Dispose()
        {
            try
            {
                ApiClient.StopStreaming();
            }
            catch (Exception ex)
            {
                EventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = ex });
                throw new UnleashException("Exception while stopping streaming", ex);
            }
        }
    }
}