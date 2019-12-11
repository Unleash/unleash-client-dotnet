using Microsoft.AspNetCore.Mvc;

namespace Sample.AspNetCore
{
    public class TestController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}