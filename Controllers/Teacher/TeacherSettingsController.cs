using Acadimy.Models;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherSettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public TeacherSettingsController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new TeacherSettingsViewModel
            {
                Grade = user.Grade,
                Specialite = user.Specialite,
                Experience = user.Experience,
                Bio = user.Bio,
                ExistingProfileImagePath = user.ProfileImagePath
            };

            return View("~/Views/Teacher/Settings/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TeacherSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ExistingProfileImagePath = (await _userManager.GetUserAsync(User))?.ProfileImagePath;
                return View("~/Views/Teacher/Settings/Index.cshtml", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.Grade = model.Grade;
            user.Specialite = model.Specialite;
            user.Experience = model.Experience;
            user.Bio = model.Bio;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.ProfileImage.CopyToAsync(stream);

                user.ProfileImagePath = $"/uploads/profiles/{fileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);

                model.ExistingProfileImagePath = user.ProfileImagePath;
                return View("~/Views/Teacher/Settings/Index.cshtml", model);
            }

            // Changement de mot de passe
            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) &&
                !string.IsNullOrWhiteSpace(model.NewPassword) &&
                !string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
            {
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("", "Les mots de passe ne correspondent pas.");
                    model.ExistingProfileImagePath = user.ProfileImagePath;
                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(
                    user, model.CurrentPassword, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    model.ExistingProfileImagePath = user.ProfileImagePath;
                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }
            }

            TempData["Success"] = "Profil mis à jour avec succès !";
            return RedirectToAction("Index");
        }
    }
}