using System;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Internal;
using Unleash.Strategies;
using Unleash.Tests.DotNetCore.Strategies;
using Xunit;

namespace Unleash.Tests.DotNetCore
{
    /// <summary>
    /// Demonstrates non-ASP.NET Core consumption of the AddUnleash extension method.
    /// </summary>
    public class UnleashConsoleTests
    {
        [Fact]
        public void Test1()
        {
            var serviceCollection = new ServiceCollection();

            // Strategies aren't really relevant to this example
            serviceCollection.AddSingleton<IStrategy, SomeStrategyNotRelevant>();
            serviceCollection
                .AddUnleash(
                    settings =>
                    {
                        settings.UnleashApi = new Uri("http://localhost/");
                        settings.AppName = "Test";
                        settings.InstanceTag = "Test";
                    });

            // In ASP.NET Core, WebHostBuilder.Build() does this implicitly.  If ASPNETCORE_ENVIRONMENT is Development,
            // this will pass true to the BuildServiceProvider.  That causes Microsoft.Extension.DependencyInjection to
            // validate that we aren't capturing a transient/scoped dependency in a singleton.  I don't think your
            // library allows us to work around this without resorting to storing & retrieving from a static such as
            // HttpContext.Current.Items
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            // This simulates what ASP.NET Core is doing.  We're doing something similar in MQ-based apps, timer-based
            // apps to get a scope per request/message/interval.
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var unleash1 = scope.ServiceProvider.GetRequiredService<IUnleash>();
                var unleash2 = scope.ServiceProvider.GetRequiredService<IUnleash>();

                Assert.Same(unleash1, unleash2); // It really is a singleton.

                unleash1.IsEnabled("x", false);

                // When the scope disposes, so will the transient/scoped IDisposable things.  Since DefaultUnleash is
                // a singleton, this doesn't dispose that.
            }

            // We do this at ASP.NET Core shutdown by hooking into IApplicationLifetime.OnStopping to explicitly
            // stop the timers & prevent ObjectDisposedExceptions in integration tests.  We could do it in a console app
            var unleashServices = serviceProvider.GetRequiredService<IUnleashServices>();
            unleashServices.Dispose();
        }
    }
}
