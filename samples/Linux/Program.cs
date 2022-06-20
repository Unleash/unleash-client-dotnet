using System;
using Unleash;
using System.Collections.Generic;
using Unleash.ClientFactory;
using System.Threading.Tasks;
using System.Threading;


namespace Linux
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = new UnleashSettings()
            {
                AppName = "dotnet-test",
                ProjectId = "default",
                InstanceTag = "instance z",
                FetchTogglesInterval = TimeSpan.FromSeconds(1),
                UnleashApi = new Uri("http://localhost:4242/api/"),
                CustomHttpHeaders = new Dictionary<String, String>()
                {
                    {"Authorization","*:development.478575d82cd97e9c0987b1770cee38eaedfcf7fadb11fcc6e818a86b" }
                }
            };
            var unleashFactory = new UnleashClientFactory();

            var unleash = new DefaultUnleash(settings);
            // var unleash = await unleashFactory.CreateClientAsync(settings, synchronousInitialization: false);

            Console.WriteLine(unleash.IsEnabled("Test"));
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine(unleash.IsEnabled("Test"));
            }
        }
    }
}
