using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Student;
using Acadimy.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public SettingsController(
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
                .Include(u => u.StudentSkills)
                .FirstOrDefaultAsync(u => u.Id == userId);

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
                NotifyAssignmentCorrection = user.NotifyAssignmentCorrection,
                NotifyNewLesson = user.NotifyNewLesson,

                Skills = user.StudentSkills.Select(s => new StudentSkillItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Percentage = s.Percentage
                }).ToList()
            };

            return View("~/Views/Student/Settings/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var user = await _userManager.Users
                .Include(u => u.StudentSkills)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            model.ExistingProfileImagePath = user.ProfileImagePath;
            model.ExistingCoverImagePath = user.CoverImagePath;

            model.Skills = model.Skills?
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .Select(s => new StudentSkillItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name!.Trim(),
                    Percentage = Math.Clamp(s.Percentage, 0, 100)
                })
                .ToList() ?? new List<StudentSkillItemViewModel>();

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Skills[")).ToList())
                ModelState.Remove(key);

            ModelState.Remove(nameof(model.CurrentPassword));
            ModelState.Remove(nameof(model.NewPassword));
            ModelState.Remove(nameof(model.ConfirmNewPassword));

            if (!ModelState.IsValid)
                return View("~/Views/Student/Settings/Index.cshtml", model);

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
            user.NotifyAssignmentCorrection = model.NotifyAssignmentCorrection;
            user.NotifyNewLesson = model.NotifyNewLesson;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImage.FileName);
                var filePath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.ProfileImage.CopyToAsync(stream);

                user.ProfileImagePath = "/uploads/profiles/" + fileName;
            }

            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.CoverImage.FileName);
                var filePath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.CoverImage.CopyToAsync(stream);

                user.CoverImagePath = "/uploads/covers/" + fileName;
            }

            // Password optional:
            // إذا كلشي عامر => نبدلو password
            // إذا خاوي أو ناقص => نتجاهلو بلا error
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
                        ModelState.AddModelError("", error.Description);

                    return View("~/Views/Student/Settings/Index.cshtml", model);
                }
            }

            _context.StudentSkills.RemoveRange(user.StudentSkills);

            foreach (var skill in model.Skills)
            {
                _context.StudentSkills.Add(new StudentSkill
                {
                    Name = skill.Name!,
                    Percentage = skill.Percentage,
                    UserId = user.Id
                });
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("~/Views/Student/Settings/Index.cshtml", model);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}