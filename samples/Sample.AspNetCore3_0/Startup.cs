using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash;

namespace Sample.AspNetCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // This is a pre-requisite for services.AddUnleash().WithDistributedToggleCollectionCache();
            // In this interest of simplicity, this uses process memory for a cache but implements the distributed
            // caching interface.  You could replace this with Redis or something to achieve distributed caching.
            services.AddDistributedMemoryCache();

            // This is a pre-requisite for services.AddUnleash().WithMemoryToggleCollectionCache();
            services.AddMemoryCache();

            // Registers core Unleash services.
            services
                .AddUnleash(
                    HostEnvironment, // Optional, sets InstanceTag from HostingEnvironment.EnvironmentName
                    Configuration.GetSection("Unleash"), // Optional, uses IConfiguration to configure Unleash
                    settings =>
                    {
                        // Optional, override bound configuration (if provided) or hard-code configuration
                    })

                // Registers all client supplied strategies
                .WithDefaultStrategies()
                // Shorthand for:
                //   .WithStrategy<DefaultStrategy>()
                //   .WithStrategy<UserWithIdStrategy>()
                //   .WithStrategy<GradualRolloutUserIdStrategy>()
                //   .WithStrategy<GradualRolloutRandomStrategy>()
                //   .WithStrategy<ApplicationHostnameStrategy>()
                //   .WithStrategy<GradualRolloutSessionIdStrategy>()
                //   .WithStrategy<RemoteAddressStrategy>();

                // Plugs in and enables Newtonsoft.Json-based serialization (recommended)
                .WithNewtonsoftJsonSerializer(
                    settings =>
                    {
                        // If you supplied an IConfiguration to the AddUnleash call above, the NewtonsoftJsonSerializer
                        // will be configured from the "Serialization:NewtonsoftJson" subtree of the path provided.
                        // i.e., Unleash:Serialization:NewtonsoftJson:Encoding = UTF8, Unleash:Serialization:NewtonsoftJson:BufferSize = 65536
                        // See appsettings.json for example.

                        // Optional: override bound configuration (if provided) or hard-code serializer configuration
                    })
                // Shorthand for:
                //   .WithJsonSerializer<NewtonsoftJsonSerializer>() (you also need to register NewtonsoftJsonSerializerSettings)

                // Replaces DefaultHttpClientFactory with HttpClientFactory-based mechanism (recommended)
                .WithHttpClientFactory(builder =>
                {
                    builder.ConfigureHttpClient((sp, httpClient) =>
                    {
                        // Modify the created HttpClient as needed.
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Sample-Header",
                            DateTimeOffset.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"));
                    });
                })

                // Register an IToggleCollectionCache implementation
                .WithDistributedToggleCollectionCache()
                // Alternatively:
                //   .WithMemoryToggleCollectionCache()
                //   .WithToggleCollectionCache<YourCustomImplementation>()

                // This gracefully shuts down Unleash when the host/application is closed
                .WithHostControlledLifetime()

                // This will block the start of the application until feature flags have been loaded from the server
                .WithSynchronousFlagLoadingOnStartup(true, TimeSpan.FromSeconds(15))

                // This registers an IUnleashAdminApiClient (if you need it)
                .WithAdminHttpClientFactory();

            services.AddControllersWithViews()
                .ConfigureApplicationPartManager(apm =>
                {
                    apm.ApplicationParts.Add(new AssemblyPart(typeof(Startup).Assembly));
                    apm.ApplicationParts.Add(new AssemblyPart(typeof(AspNetCoreCommonAssemblyHandle).Assembly));
                    apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(typeof(AspNetCoreCommonAssemblyHandle).Assembly));
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
