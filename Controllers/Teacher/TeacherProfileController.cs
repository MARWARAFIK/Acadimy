using Acadimy.Models;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new TeacherProfileViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? "",
                Bio = user.Bio ?? "Aucune biographie disponible.",
                Specialite = user.Specialite ?? "Non spécifiée",
                Grade = user.Grade ?? "Non spécifié",
                Experience = user.Experience,
                ProfileImagePath = user.ProfileImagePath ?? "/images/default-profile.png",
                SkillsList = (user.Skill ?? "")
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .ToList()
            };

            return View("~/Views/Teacher/Profile/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> TeacherProfile()
        {
            return RedirectToAction("Index");
        }
    }
}