using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash.Tests.DotNetCore.AspNetCore.Mvc;
using Unleash.Tests.DotNetCore.Strategies;

namespace Unleash.Tests.AspNetCore.Common
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUnleash(
                    HostingEnvironment,
                    settings =>
                    {
                        settings.UnleashApi = new Uri("http://localhost:4242/");
                        settings.AppName = "MyWebApplication";
                        settings.InstanceTag = "Test";
                    }
                )
                .WithDefaultStrategies()
                .WithStrategy<SomeStrategyNotRelevant>()
                .WithWebHostControlledLifetime()
                .WithSynchronousFlagLoadingOnStartup();

            services.AddMvc(options =>
            {
                // options.EnableEndpointRouting = false;

                // Hook up an MVC-layer filter that can add MVC-level concepts to the
                // IUnleashContextProvider.Context.Properties
                options.Filters.Add<UnleashMvcActionFilter>();
            }).AddApplicationPart(Assembly.GetExecutingAssembly());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            // Middleware at the ASP.NET Core layer can alter the request-scoped IUnleashContextProvider
            app.Use((ctx, next) =>
            {
                var ctxProvider = ctx.RequestServices.GetRequiredService<IUnleashContextProvider>();
                ctxProvider.Context.Properties["Method"] = ctx.Request.Method;
                ctxProvider.Context.Properties["Path"] = ctx.Request.Path;
                return next();
            });

            // ActionFilters in ASP.NET Core MVC (higher-level than middleware) can alter the same request-scoped
            // IUnleashContextProvider on the same request.  See UnleashMvcActionFilter.
            app.UseMvc();

            // An ASP.NET Core MVC controller action should see all of the properties (Method, Path, ActionDisplayName).
            // Lower-level middleware would only see Method + Path
        }
    }
}
