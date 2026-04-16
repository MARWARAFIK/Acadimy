using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    [Authorize(Roles = "Student")]
    public IActionResult IndexStudent()
    {
        return View();
    }

    [Authorize(Roles = "Teacher")]
    public IActionResult IndexTeacher()
    {
        return View();
    }
}