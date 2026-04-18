using Microsoft.AspNetCore.Mvc;

namespace Acadimy.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexStudent()
        {
            return View();
        }

        public IActionResult IndexTeacher()
        {
            return View();
        }
    }
}