using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash;
using Unleash.Internal;

namespace Sample.Console.DependencyInjection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Optional Arguments: [userId] [sessionId] [featureToggleName] [extraParamName extraParamValue]");

            var configurationRoot = ConfigureConfiguration();
            var unleashConfigurationRoot = configurationRoot.GetSection("Unleash");

            using (var serviceProvider = ConfigureServices(unleashConfigurationRoot))
            {
                System.Console.WriteLine("Waiting for initial feature toggle load to complete...");
                var unleashServices = serviceProvider.GetRequiredService<IUnleashServices>();
                await unleashServices.FeatureToggleLoadComplete(false, CancellationToken.None);
                System.Console.WriteLine("Feature toggles loaded.");

                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                for (var x = 0; x < 3; x++)
                {
                    System.Console.WriteLine($"Beginning simulated request scope ({x + 1} of 3)...");
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var unleashContextProvider =
                            scope.ServiceProvider.GetRequiredService<IUnleashContextProvider>();
                        var unleash = scope.ServiceProvider.GetRequiredService<IUnleash>();

                        if (unleashContextProvider.Context.Properties.Count > 0)
                        {
                            throw new InvalidOperationException(
                                "Each scope gets its own context.  Context should not bleed across requests.");
                        }

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

                        System.Console.WriteLine("Disposing request scope.");
                    }
                }

                // Allow metrics to get sent.
                System.Console.WriteLine("Waiting for metrics job to execute...");
                await Task.Delay(TimeSpan.FromSeconds(2));
                System.Console.WriteLine("Metrics job complete (presumably).");

                System.Console.WriteLine("Stopping scheduler...");
            }
            System.Console.WriteLine("Stopped.");
        }

        private static IConfigurationRoot ConfigureConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            configurationBuilder.AddEnvironmentVariables();
            return configurationBuilder.Build();
        }

        private static ServiceProvider ConfigureServices(IConfiguration unleashConfigurationRoot)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddUnleash(
                    unleashConfigurationRoot,
                    settings => { })
                .WithDefaultStrategies()
                .WithNewtonsoftJsonSerializer()
                .WithHttpClientFactory();

            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            ValidateAppSettings(serviceProvider);

            return serviceProvider;
        }

        private static string GetIpAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address.ToString())
                .FirstOrDefault();
        }

        private static void ValidateAppSettings(ServiceProvider serviceProvider)
        {
            // These are sanity checks to ensure ConfigurationBuilder + appsettings.json's Unleash section made it
            // through to the Unleash client.

            var settings = serviceProvider.GetRequiredService<UnleashSettings>();

            if (settings.AppName != "Sample.Console.DependencyInjection")
            {
                throw new Exception("Expected appsettings.json to set AppName to 'Sample.Console.DependencyInjection'");
            }

            if (!settings.UnleashApi.Equals(new Uri("http://localhost:4242/")))
            {
                throw new Exception(
                    "Expected appsettings.json to set UnleashApi to 'http://localhost:4242/' (docker-compose URL)");
            }

            if (settings.InstanceTag != "Test")
            {
                throw new Exception("Expected appsettings.json to set InstanceTag to 'Sample.Console.DependencyInjection'");
            }

            if (!settings.FetchTogglesInterval.Equals(TimeSpan.FromSeconds(30)))
            {
                throw new Exception("Expected appsettings.json to set FetchTogglesInterval to '00:00:30'");
            }

            if (!settings.SendMetricsInterval.Equals(TimeSpan.FromSeconds(1)))
            {
                throw new Exception("Expected appsettings.json to set SendMetricsInterval to '00:00:01'");
            }
        }
    }
}
