using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadimy.Controllers.Student
{
    [Authorize(Roles = "Étudiant")]
    public class StudentController : Controller
    {
        public IActionResult Index() => View("~/Views/Home/Student/Index.cshtml");
    }
}