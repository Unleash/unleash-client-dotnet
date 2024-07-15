using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Unleash;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            var unleash = new DefaultUnleash(new UnleashSettings()
            {
                UnleashApi = new Uri("http://localhost:4242/api"),
                AppName = "variant-sample",
                InstanceTag = "instance 1",
                SendMetricsInterval = TimeSpan.FromSeconds(10),
                FetchTogglesInterval = TimeSpan.FromSeconds(10),
            });
            unleash.ConfigureEvents(evtCfg =>
            {
                evtCfg.ImpressionEvent = evt => { Console.WriteLine(evt.FeatureName); };
                evtCfg.ErrorEvent = evt => { Console.WriteLine(evt.ErrorType + "-" + evt.Error?.Message); };
            });
            services.AddSingleton<IUnleash>(c => unleash);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}