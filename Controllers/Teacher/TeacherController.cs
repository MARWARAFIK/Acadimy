using Acadimy.Data;
using Acadimy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var courses = await _context.TeacherCourses
                .Where(c => c.UserId == userId && !c.IsArchived)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View("~/Views/Home/Teacher/Index.cshtml", courses);
        }
    }
}