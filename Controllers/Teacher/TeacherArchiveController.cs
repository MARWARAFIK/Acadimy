using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherArchiveController(
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

            var archivedPosts = await _context.TeacherPosts
                .Where(p => p.UserId == user.Id && p.IsArchived)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new TeacherPostViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Content = p.Content,
                    ImagePath = p.ImagePath,
                    CreatedAt = p.CreatedAt,
                    FullName = p.User != null
                        ? (p.User.FirstName + " " + p.User.LastName).Trim()
                        : "Unknown User",
                    ProfileImagePath = p.User != null && !string.IsNullOrEmpty(p.User.ProfileImagePath)
                        ? p.User.ProfileImagePath
                        : "/images/default-user.png",
                    IsArchived = p.IsArchived
                })
                .ToListAsync();

            var archivedCourses = await _context.TeacherCourses
                .Where(c => c.UserId == user.Id && c.IsArchived)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new TeacherCourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Category = c.Category,
                    Level = c.Level,
                    Description = c.Description,
                    ExistingThumbnailPath = c.ThumbnailPath,
                    ExistingFilePath = c.FilePath,
                    IsArchived = c.IsArchived,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var model = new TeacherArchiveViewModel
            {
                ArchivedPosts = archivedPosts,
                ArchivedCourses = archivedCourses
            };

            return View("~/Views/Teacher/Archive/Index.cshtml", model);
        }
    }
}