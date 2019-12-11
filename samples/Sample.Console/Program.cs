using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unleash;
using Unleash.Internal;

namespace Sample.Console
{
    class Program
    {
        /// <summary>
        /// This pattern is not recommended.  You will miss out on the Unleash.Extensions.* capabilities.  You should
        /// consider using the Generic Host if possible or minimally introducing dependency injection.  See the other
        /// sample projects for examples.  This shows the bare functionality most closely resembling the original
        /// Unleash .NET client (pre-.NET Core)
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Optional Arguments: [userId] [sessionId] [featureToggleName] [extraParamName extraParamValue]");

            var settings = new UnleashSettings
            {
                UnleashApi = new Uri("http://localhost:4242/"),
                AppName = "Sample.Console",
                InstanceTag = "Test",
                FetchTogglesInterval = TimeSpan.FromSeconds(30),
                SendMetricsInterval = TimeSpan.FromSeconds(1)
            };

            var unleashContext = new UnleashContext();

            using (var unleashServices = new DefaultUnleashServices(settings))
            {
                var unleashContextProvider = new DefaultUnleashContextProvider(unleashContext);
                var unleash = new Unleash.Unleash(settings, unleashServices, unleashContextProvider);

                System.Console.WriteLine("Waiting for initial feature toggle load to complete...");
                await unleashServices.FeatureToggleLoadComplete(false, CancellationToken.None);
                System.Console.WriteLine("Feature toggles loaded.");

                // Configure settings from args/defaults
                var userId = args.Length > 0 ? args[0] : "1";
                var sessionId = args.Length > 1 ? args[1] : "1";
                var featureToggleName = args.Length > 2 ? args[2] : "unleash.sample.test.flag";
                var extraParamName = args.Length > 4 ? args[3] : null;
                var extraParamValue = args.Length > 4 ? args[4] : null;

                // Initialize context
                unleashContextProvider.Context.RemoteAddress = GetIpAddress() ?? "127.0.0.1";
                unleashContextProvider.Context.UserId = userId;
                unleashContextProvider.Context.SessionId = sessionId;
                if (extraParamName != null && extraParamValue != null)
                {
                    unleashContextProvider.Context.Properties[extraParamName] = extraParamValue;
                }

                // Check flag
                var isEnabled = unleash.IsEnabled(featureToggleName);

                // Output results
                var messageBuilder = new StringBuilder();
                messageBuilder.Append($"{featureToggleName}={isEnabled} for ");
                messageBuilder.Append($"(UserId={userId},SessionId={sessionId}");

                if (extraParamName != null && extraParamValue != null)
                {
                    messageBuilder.Append($",{extraParamName}={extraParamValue})");
                }
                else
                {
                    messageBuilder.Append(")");
                }

                System.Console.WriteLine(messageBuilder.ToString());

                // Allow metrics to get sent.
                System.Console.WriteLine("Waiting for metrics job to execute...");
                await Task.Delay(TimeSpan.FromSeconds(2));
                System.Console.WriteLine("Metrics job complete (presumably).");

                System.Console.WriteLine("Stopping scheduler...");
            }
            System.Console.WriteLine("Stopped.");
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
