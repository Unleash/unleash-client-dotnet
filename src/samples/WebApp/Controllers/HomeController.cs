using System.Web.Mvc;
using Unleash;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnleash unleash;

        public HomeController()
        {
            unleash = WebApiApplication.Unleash;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Unleash";

            ViewBag.Feature = "Demo123";
            ViewBag.FeatureEnabled = unleash.IsEnabled(ViewBag.Feature);

            return View();
        }
    }
}
