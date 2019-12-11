using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.GenericHost.HostedServices;
using Unleash;

namespace Sample.GenericHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Optional Arguments: [sessionId] [featureToggleName] [extraParamName extraParamValue]");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    // Turn down the HTTP-related log noise
                    loggingBuilder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    // Load configuration from appsettings.json + Environment Variables
                    configurationBuilder.AddJsonFile("appsettings.json", false, true);
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var hostingEnvironment = hostContext.HostingEnvironment;

                    var configuration = hostContext.Configuration;
                    var unleashConfiguration = configuration.GetSection("Unleash");

                    // Add Unleash services
                    services.AddUnleash(hostingEnvironment, unleashConfiguration)
                        .WithDefaultStrategies()
                        .WithNewtonsoftJsonSerializer()
                        .WithMemoryToggleCollectionCache()
                        .WithHttpClientFactory()
                        .WithSynchronousFlagLoadingOnStartup()
                        .WithHostControlledLifetime();

                    // Configure a recurring job to check feature flag value in a loop
                    var sessionId = args.Length > 0 ? args[0] : "1";
                    var featureToggleName = args.Length > 1 ? args[1] : "unleash.sample.test.flag";
                    var extraParamName = args.Length > 3 ? args[2] : null;
                    var extraParamValue = args.Length > 3 ? args[3] : null;

                    services.AddTimedService<UnleashSampleService, UnleashSampleServiceOptions, UnleashSampleServiceState>(
                        opt =>
                        {
                            opt.Interval = TimeSpan.FromSeconds(30);
                            opt.MissedIntervalHandling = MissedIntervalHandling.RunImmediately;

                            opt.SessionId = sessionId;
                            opt.FeatureToggleName = featureToggleName;
                            opt.ExtraParamName = extraParamName;
                            opt.ExtraParamValue = extraParamValue;
                        });
                });
        }
    }
}
