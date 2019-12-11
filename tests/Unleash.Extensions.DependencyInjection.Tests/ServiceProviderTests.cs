using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Unleash.Extensions.DependencyInjection.Tests
{
    public class ServiceProviderTests
    {
        [Fact]
        public void AddUnleash_RegistersNecessaryServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUnleash(s =>
            {
                s.UnleashApi = new Uri("http://localhost:4242/");
                s.AppName = "Test";
                s.InstanceTag = "Test";
            });

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var unleash = scope.ServiceProvider.GetRequiredService<IUnleash>();
                Assert.IsType<Unleash>(unleash);
            }
        }
    }
}
