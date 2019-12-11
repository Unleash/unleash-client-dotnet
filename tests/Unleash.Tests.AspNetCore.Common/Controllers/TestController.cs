using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Unleash.Tests.DotNetCore.AspNetCore.Controllers
{
    public class TestController : Controller
    {
        private IUnleash Unleash { get; }
        private IUnleashContextProvider ContextProvider { get; }

        public TestController(IUnleash unleash, IUnleashContextProvider contextProvider)
        {
            Unleash = unleash;
            ContextProvider = contextProvider;
        }

        [Route("FlagTest")]
        public ActionResult FlagTest()
        {
            if (!Unleash.IsEnabled("unleash.client.test.integration.flag"))
            {
                return Conflict();
            }

            return Ok("Ok");
        }

        [Route("ContextProviderKeys")]
        public ActionResult<Dictionary<string, string>> ContextProviderKeys()
        {
            return ContextProvider.Context.Properties;
        }
    }
}
