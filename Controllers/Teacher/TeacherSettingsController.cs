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
    public class TeacherSettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public TeacherSettingsController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
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

            var model = new TeacherSettingsViewModel
            {
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                Grade = user.Grade,
                Specialite = user.Specialite,
                Experience = user.Experience,
                Bio = user.Bio,
                ExistingProfileImagePath = user.ProfileImagePath,
                ExistingCoverImagePath = user.CoverImagePath,

                NotifyStudentSubmission = user.NotifyStudentSubmission,
                NotifyCourseActivity = user.NotifyCourseActivity,
                NotifyAnnouncement = user.NotifyAnnouncement,
                NotifyMessages = user.NotifyMessages,

                ChangePassword = false,

                Expertises = user.TeacherExpertises
                    .Select(e => new TeacherExpertiseItemViewModel
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Percentage = e.Percentage
                    })
                    .ToList()
            };

            return View("~/Views/Teacher/Settings/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TeacherSettingsViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.Users
                .Include(u => u.TeacherExpertises)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            model.ExistingProfileImagePath = user.ProfileImagePath;
            model.ExistingCoverImagePath = user.CoverImagePath;

            ModelState.Remove(nameof(TeacherSettingsViewModel.CurrentPassword));
            ModelState.Remove(nameof(TeacherSettingsViewModel.NewPassword));
            ModelState.Remove(nameof(TeacherSettingsViewModel.ConfirmNewPassword));

            model.Expertises = model.Expertises?
                .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                .Select(e => new TeacherExpertiseItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name?.Trim(),
                    Percentage = e.Percentage
                })
                .ToList() ?? new List<TeacherExpertiseItemViewModel>();

            var expertiseKeys = ModelState.Keys
                .Where(k => k.StartsWith("Expertises["))
                .ToList();

            foreach (var key in expertiseKeys)
                ModelState.Remove(key);

            if (!ModelState.IsValid)
                return View("~/Views/Teacher/Settings/Index.cshtml", model);

            if (model.ChangePassword)
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(TeacherSettingsViewModel.CurrentPassword), "Current password is required.");
                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    ModelState.AddModelError(nameof(TeacherSettingsViewModel.NewPassword), "New password is required.");
                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }

                if (string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
                {
                    ModelState.AddModelError(nameof(TeacherSettingsViewModel.ConfirmNewPassword), "Please confirm the new password.");
                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }

                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError(nameof(TeacherSettingsViewModel.ConfirmNewPassword), "Password confirmation does not match.");
                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }
            }

            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();
            user.Grade = model.Grade?.Trim();
            user.Specialite = model.Specialite?.Trim();
            user.Experience = model.Experience;
            user.Bio = model.Bio?.Trim();

            user.NotifyStudentSubmission = model.NotifyStudentSubmission;
            user.NotifyCourseActivity = model.NotifyCourseActivity;
            user.NotifyAnnouncement = model.NotifyAnnouncement;
            user.NotifyMessages = model.NotifyMessages;

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var newEmail = model.Email.Trim();

                if (user.Email != newEmail)
                {
                    var userNameResult = await _userManager.SetUserNameAsync(user, newEmail);
                    if (!userNameResult.Succeeded)
                    {
                        foreach (var error in userNameResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);

                        return View("~/Views/Teacher/Settings/Index.cshtml", model);
                    }

                    var emailResult = await _userManager.SetEmailAsync(user, newEmail);
                    if (!emailResult.Succeeded)
                    {
                        foreach (var error in emailResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);

                        return View("~/Views/Teacher/Settings/Index.cshtml", model);
                    }
                }
            }

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                user.ProfileImagePath = await SaveImageAsync(model.ProfileImage, "uploads/teachers/profiles");

            if (model.CoverImage != null && model.CoverImage.Length > 0)
                user.CoverImagePath = await SaveImageAsync(model.CoverImage, "uploads/teachers/covers");

            if (model.ChangePassword)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword!,
                    model.NewPassword!);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View("~/Views/Teacher/Settings/Index.cshtml", model);
                }
            }

            _context.TeacherExpertises.RemoveRange(user.TeacherExpertises);

            foreach (var expertise in model.Expertises)
            {
                _context.TeacherExpertises.Add(new TeacherExpertise
                {
                    Name = expertise.Name!.Trim(),
                    Percentage = expertise.Percentage,
                    UserId = user.Id
                });
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("~/Views/Teacher/Settings/Index.cshtml", model);
            }

            await _context.SaveChangesAsync();

            var refreshedExpertises = await _context.TeacherExpertises
                .Where(e => e.UserId == user.Id)
                .Select(e => new TeacherExpertiseItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Percentage = e.Percentage
                })
                .ToListAsync();

            model.Expertises = refreshedExpertises;
            model.ExistingProfileImagePath = user.ProfileImagePath;
            model.ExistingCoverImagePath = user.CoverImagePath;
            model.ChangePassword = false;
            model.CurrentPassword = null;
            model.NewPassword = null;
            model.ConfirmNewPassword = null;

            ViewBag.Message = "Vos informations ont été mises à jour avec succès.";

            return View("~/Views/Teacher/Settings/Index.cshtml", model);
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folderPath)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, folderPath);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/" + folderPath.Replace("\\", "/") + "/" + fileName;
        }
    }
}