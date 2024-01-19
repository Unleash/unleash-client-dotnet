using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Unleash.Events;
using System.Net.Http;

namespace Unleash.Internal
{
    internal class FetchFeatureToggles
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureToggles));
        private readonly EventCallbackConfig eventConfig;
        private readonly IUnleashApiClient apiClient;
        private string Etag { get; set; }

        public FetchFeatureToggles(
            IUnleashApiClient apiClient,
            EventCallbackConfig eventConfig)
        {
            this.apiClient = apiClient;
            this.eventConfig = eventConfig;
        }

        public async Task<FetchTogglesResult> FetchToggles(CancellationToken cancellationToken)
        {
            FetchTogglesResult result;
            try
            {
                result = await apiClient.FetchToggles(Etag, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(() => $"UNLEASH: Unhandled exception when fetching toggles.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = ex });
                throw new UnleashException("Exception while fetching from API", ex);
            }

            if (!result.HasChanged)
            {
                return result;
            }

            if (string.IsNullOrEmpty(result.Etag))
            {
                result.HasChanged = false;
                return result;
            }

            if (result.Etag == Etag)
            {
                result.HasChanged = false;
                return result;             
            }

            return result;
        }
    }
}