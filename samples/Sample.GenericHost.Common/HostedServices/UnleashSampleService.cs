using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unleash;

namespace Sample.GenericHost.HostedServices
{
    public class UnleashSampleService : ITimerInvokedService<UnleashSampleServiceState>
    {
        public UnleashSampleServiceState State { get; }
        private IUnleashContextProvider UnleashContextProvider { get; }
        private IUnleash Unleash { get; }
        private ILogger Logger { get; }
        private UnleashSampleServiceOptions Options { get; }

        public UnleashSampleService(
            UnleashSampleServiceState state,
            IUnleashContextProvider unleashContextProvider,
            IUnleash unleash,
            IOptions<UnleashSampleServiceOptions> options,
            ILogger<UnleashSampleService> logger)
        {
            State = state;
            UnleashContextProvider = unleashContextProvider;
            Unleash = unleash;
            Options = options.Value;
            Logger = logger;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Initialize context
            UnleashContextProvider.Context.RemoteAddress = GetIpAddress() ?? "127.0.0.1";
            UnleashContextProvider.Context.UserId = State.Iteration.ToString();
            UnleashContextProvider.Context.SessionId = Options.SessionId;
            if (Options.ExtraParamName != null && Options.ExtraParamValue != null)
            {
                UnleashContextProvider.Context.Properties[Options.ExtraParamName] = Options.ExtraParamValue;
            }

            // Check flag
            var isEnabled = Unleash.IsEnabled(Options.FeatureToggleName);

            // Output results
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"{Options.FeatureToggleName}={isEnabled} for ");
            messageBuilder.Append($"(UserId={State.Iteration},SessionId={Options.SessionId}");

            if (Options.ExtraParamName != null && Options.ExtraParamValue != null)
            {
                messageBuilder.Append($",{Options.ExtraParamName}={Options.ExtraParamValue})");
            }
            else
            {
                messageBuilder.Append(")");
            }

            Logger.LogInformation(messageBuilder.ToString());

            State.Iteration++;

            return Task.CompletedTask;
        }

        private static string GetIpAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address.ToString())
                .FirstOrDefault();
        }
    }
}
