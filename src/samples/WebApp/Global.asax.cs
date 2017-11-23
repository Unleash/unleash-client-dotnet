using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unleash;
using Unleash.Util;

namespace WebApp
{
    public class WebApiApplication : HttpApplication
    {
        public static IUnleash Unleash { get; private set; }
        public static UnleashConfig UnleashConfig { get; private set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            UnleashConfig = new UnleashConfig()
                .SetAppName("dotnet-test")
                .SetInstanceId("instance 1")
                .SetFetchTogglesInterval(TimeSpan.FromSeconds(20))
                .SetSendMetricsInterval(TimeSpan.FromSeconds(10))
                .EnableMetrics()
                .UnleashContextProvider(new AspNetContextProvider())
                //.SetUnleashApi("http://localhost:4242/")
                .SetUnleashApi("http://unleash.herokuapp.com/")
                //.SetJsonSerializer(new JsonNetSerializer());
                ;
            
            Unleash = new DefaultUnleash(UnleashConfig);
        }

        protected void Application_End()
        {
            Unleash.Dispose();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Items["UnleashContext"] = new UnleashContext.Builder()
                    .UserId("ABC")
                    .SessionId(HttpContext.Current.Session?.SessionID)
                    .RemoteAddress(HttpContext.Current.Request.UserHostAddress)
                    .AddProperty("UserRoles", "A, B, C")
                    .Build()
                ;
        }
    }

    public class AspNetContextProvider : IUnleashContextProvider
    {
        public UnleashContext Context => 
            HttpContext.Current?.Items["UnleashContext"] as UnleashContext;
    }
}