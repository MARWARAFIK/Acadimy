using Acadimy.Models;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.Users
                .Include(u => u.TeacherExpertises)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new TeacherProfileViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? "",
                Bio = string.IsNullOrWhiteSpace(user.Bio) ? "Aucune biographie disponible." : user.Bio,
                Specialite = string.IsNullOrWhiteSpace(user.Specialite) ? "Non spécifiée" : user.Specialite,
                Grade = string.IsNullOrWhiteSpace(user.Grade) ? "Non spécifié" : user.Grade,
                Experience = user.Experience,
                ProfileImagePath = string.IsNullOrWhiteSpace(user.ProfileImagePath)
                    ? "/images/default-user.png"
                    : user.ProfileImagePath,
                CoverImagePath = string.IsNullOrWhiteSpace(user.CoverImagePath)
                    ? "/images/teacher-banner-default.jpg"
                    : user.CoverImagePath,
                Expertises = user.TeacherExpertises
                    .Select(e => new TeacherExpertiseItemViewModel
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Percentage = e.Percentage
                    })
                    .ToList()
            };

            return View("~/Views/Teacher/Profile/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult TeacherProfile()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}