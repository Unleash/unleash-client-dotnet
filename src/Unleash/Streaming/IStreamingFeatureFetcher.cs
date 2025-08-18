using System;
using System.Threading.Tasks;
using LaunchDarkly.EventSource;

namespace Unleash.Streaming
{
    public interface IStreamingFeatureFetcher : IDisposable
    {
        Task StartAsync();
    }

    public interface IStreamingEventHandler
    {
        void HandleMessage(object target, MessageReceivedEventArgs data);
        void HandleError(object target, ExceptionEventArgs data);
    }
}