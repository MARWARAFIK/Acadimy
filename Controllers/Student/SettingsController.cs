using Acadimy.Models;
using Acadimy.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new SettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumberCustom = user.PhoneNumberCustom,
                Location = user.Location,
                Website = user.Website,
                Bio = user.Bio,
                Filiere = user.Filiere,
                Niveau = user.Niveau,
                ExistingProfileImagePath = user.ProfileImagePath,
                ExistingCoverImagePath = user.CoverImagePath,
                NotifyNewCourse = user.NotifyNewCourse,
                NotifyApplicationStatus = user.NotifyApplicationStatus,
                NotifyAnnouncement = user.NotifyAnnouncement,
                NotifyMessages = user.NotifyMessages,
                Skill = user.Skill,
                SkillPercent = user.SkillPercent
            };

            return View("~/Views/Student/Settings/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.ExistingProfileImagePath = user.ProfileImagePath;
                model.ExistingCoverImagePath = user.CoverImagePath;
                return View("~/Views/Student/Settings/Index.cshtml", model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumberCustom = model.PhoneNumberCustom;
            user.Location = model.Location;
            user.Website = model.Website;
            user.Bio = model.Bio;
            user.Filiere = model.Filiere;
            user.Niveau = model.Niveau;

            user.NotifyNewCourse = model.NotifyNewCourse;
            user.NotifyApplicationStatus = model.NotifyApplicationStatus;
            user.NotifyAnnouncement = model.NotifyAnnouncement;
            user.NotifyMessages = model.NotifyMessages;

            user.Skill = model.Skill;
            user.SkillPercent = model.SkillPercent;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string profileFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(profileFolder);

                string profileFileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImage.FileName);
                string profileFilePath = Path.Combine(profileFolder, profileFileName);

                using (var stream = new FileStream(profileFilePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                user.ProfileImagePath = "/uploads/profiles/" + profileFileName;
            }

            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                string coverFolder = Path.Combine(_environment.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(coverFolder);

                string coverFileName = Guid.NewGuid() + Path.GetExtension(model.CoverImage.FileName);
                string coverFilePath = Path.Combine(coverFolder, coverFileName);

                using (var stream = new FileStream(coverFilePath, FileMode.Create))
                {
                    await model.CoverImage.CopyToAsync(stream);
                }

                user.CoverImagePath = "/uploads/covers/" + coverFileName;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                model.ExistingProfileImagePath = user.ProfileImagePath;
                model.ExistingCoverImagePath = user.CoverImagePath;
                return View("~/Views/Student/Settings/Index.cshtml", model);
            }

            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) &&
                !string.IsNullOrWhiteSpace(model.NewPassword) &&
                !string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    model.ExistingProfileImagePath = user.ProfileImagePath;
                    model.ExistingCoverImagePath = user.CoverImagePath;
                    return View("~/Views/Student/Settings/Index.cshtml", model);
                }
            }

            ViewBag.Message = "Settings updated successfully.";
            model.ExistingProfileImagePath = user.ProfileImagePath;
            model.ExistingCoverImagePath = user.CoverImagePath;

            model.CurrentPassword = "";
            model.NewPassword = "";
            model.ConfirmNewPassword = "";

            return View("~/Views/Student/Settings/Index.cshtml", model);
        }
    }
}