using Acadimy.Models;
using Acadimy.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new ProfileViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Phone = user.PhoneNumberCustom,
                Location = user.Location,
                Website = user.Website,
                Bio = user.Bio,
                Filiere = user.Filiere,
                Niveau = user.Niveau,
                ProfileImagePath = user.ProfileImagePath,
                CoverImagePath = user.CoverImagePath,
                Skill = user.Skill,
                SkillPercent = user.SkillPercent
            };

            return View("~/Views/Student/Profile/Index.cshtml", model);
        }
    }
}