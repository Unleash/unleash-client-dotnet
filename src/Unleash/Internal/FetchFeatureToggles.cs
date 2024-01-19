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

        public async Task<Tuple<ToggleCollection, string, bool>> FetchToggles(CancellationToken cancellationToken)
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
                return new Tuple<ToggleCollection, string, bool>(null, string.Empty, false);
            }

            if (string.IsNullOrEmpty(result.Etag))
                return new Tuple<ToggleCollection, string, bool>(null, string.Empty, false);

            if (result.Etag == Etag)
                return new Tuple<ToggleCollection, string, bool>(null, string.Empty, false);

            return new Tuple<ToggleCollection, string, bool>(result.ToggleCollection, result.Etag, true);
        }
    }
}