using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unleash;

namespace WebApp
{
    public class WebApiApplication : HttpApplication
    {
        public static IUnleash Unleash { get; private set; }
        public static UnleashSettings UnleashSettings { get; private set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            UnleashSettings = new UnleashSettings()
            {
                UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
                //UnleashApi = new Uri("http://localhost:4242/api/"),
                AppName = "dotnet-api-test",
                InstanceTag = "instance 1",
                SendMetricsInterval = TimeSpan.FromSeconds(20),
                UnleashContextProvider = new AspNetContextProvider(),
                //JsonSerializer = new JsonNetSerializer()
            };

            UnleashInfo = UnleashSettings.ToString();
            
            Unleash = new DefaultUnleash(UnleashSettings);
        }

        public static string UnleashInfo;

        protected void Application_End()
        {
            Unleash.Dispose();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Items["UnleashContext"] = new UnleashContext
            {
                UserId = "ABC",
                SessionId = HttpContext.Current.Session?.SessionID,
                RemoteAddress = HttpContext.Current.Request.UserHostAddress,
                Properties = new Dictionary<string, string>()
                {
                    {"UserRoles", "A, B, C"}
                }
            };
        }
    }

    public class AspNetContextProvider : IUnleashContextProvider
    {
        public UnleashContext Context => 
            HttpContext.Current?.Items["UnleashContext"] as UnleashContext;
    }
}