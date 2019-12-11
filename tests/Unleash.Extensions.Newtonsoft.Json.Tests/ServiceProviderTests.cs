using Microsoft.Extensions.DependencyInjection;
using Unleash.Serialization;
using Xunit;

namespace Unleash.Extensions.Newtonsoft.Json.Tests
{
    public class ServiceProviderTests
    {
        [Fact]
        public void AddUnleash_RegistersNecessaryServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUnleash();

            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var jsonSerializer = serviceProvider.GetRequiredService<IJsonSerializer>();
            Assert.IsType<NewtonsoftJsonSerializer>(jsonSerializer);
        }
    }
}
