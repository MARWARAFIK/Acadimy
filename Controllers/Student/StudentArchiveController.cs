using Acadimy.Data;
using Acadimy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    public class StudentArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentArchiveController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var posts = await _context.StudentPosts
                .Where(p => p.UserId == user.Id && p.IsArchived)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View("~/Views/Student/Archive/Index.cshtml", posts);
        }
    }
}