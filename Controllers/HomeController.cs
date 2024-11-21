using Microsoft.AspNetCore.Mvc;

namespace YourProjectNamespace.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Rooms()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
