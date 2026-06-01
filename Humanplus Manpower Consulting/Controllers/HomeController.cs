using Microsoft.AspNetCore.Mvc;

namespace Humanplus_Manpower_Consulting.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult About() => View();

        public IActionResult Services() => View();

        public IActionResult Contact() => View();

        public IActionResult Privacy() => View();
    }
}
