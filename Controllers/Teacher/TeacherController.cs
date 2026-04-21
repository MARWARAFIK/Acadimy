using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherController : Controller
    {
        public IActionResult Index() => View("~/Views/Home/Teacher/Index.cshtml");
    }
}