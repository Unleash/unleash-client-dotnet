using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Unleash.Extensions.DependencyInjection.Tests.Helpers
{
    public class UnleashConfigurationBuilder
    {
        private readonly IDictionary<string, string> _configuration = new Dictionary<string, string>();

        private UnleashConfigurationBuilder(Uri unleashApi = null, string appName = "Test", string instanceTag = "Test", TimeSpan? fetchTogglesInterval = null, TimeSpan? sendMetricsInterval = null)
        {
            _configuration["Unleash:UnleashApi"] = unleashApi.ToString();
            _configuration["Unleash:AppName"] = appName;
            _configuration["Unleash:InstanceTag"] = instanceTag;
            _configuration["Unleash:FetchTogglesInterval"] = fetchTogglesInterval.ToString();
            _configuration["Unleash:SendMetricsInterval"] = sendMetricsInterval.ToString();
        }

        public static UnleashConfigurationBuilder Create(Uri unleashApi = null, string appName = "Test", string instanceTag = "Test", TimeSpan? fetchTogglesInterval = null, TimeSpan? sendMetricsInterval = null)
        {
            if (unleashApi == null)
            {
                unleashApi = new Uri("http://localhost:4242/");
            }

            if (fetchTogglesInterval == null)
            {
                fetchTogglesInterval = TimeSpan.FromSeconds(30);
            }

            return new UnleashConfigurationBuilder(unleashApi, appName, instanceTag, fetchTogglesInterval, sendMetricsInterval);
        }

        public UnleashConfigurationBuilder AddSection(string keyNamespace, params (string, string)[] configuration)
        {
            return AddSection(
                keyNamespace,
                configuration.Select(x => new KeyValuePair<string, string>(x.Item1, x.Item2)).ToArray()
            );
        }

        public UnleashConfigurationBuilder AddSection(string keyNamespace, IEnumerable<KeyValuePair<string, string>> configuration)
        {
            if (keyNamespace.StartsWith("Unleash:", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var kvp in configuration)
                {
                    _configuration[$"{keyNamespace}:{kvp.Key}"] = kvp.Value;
                }
            }
            else
            {
                foreach (var kvp in configuration)
                {
                    _configuration[$"Unleash:{keyNamespace}:{kvp.Key}"] = kvp.Value;
                }
            }

            return this;
        }

        public IConfiguration Build()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(_configuration);

            var configuration = configurationBuilder.Build();
            var unleashConfiguration = configuration.GetSection("Unleash");
            return unleashConfiguration;
        }
    }
}
