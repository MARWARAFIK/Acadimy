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
    [Route("TeacherCourses")]
    public class TeacherCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public TeacherCoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var courses = await _context.TeacherCourses
                .Where(c => c.UserId == user.Id && !c.IsArchived)
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

            return View("~/Views/Teacher/Courses/Index.cshtml", courses);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Teacher/Courses/Create.cshtml", new TeacherCourseViewModel());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCourseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View("~/Views/Teacher/Courses/Create.cshtml", model);

            string? thumbnailPath = null;
            string? filePath = null;

            if (model.Thumbnail != null && model.Thumbnail.Length > 0)
            {
                var imageFolder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-courses", "images");

                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Thumbnail.FileName)}";
                var imageFullPath = Path.Combine(imageFolder, imageName);

                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    await model.Thumbnail.CopyToAsync(stream);
                }

                thumbnailPath = "/uploads/teacher-courses/images/" + imageName;
            }

            if (model.CourseFile != null && model.CourseFile.Length > 0)
            {
                var fileFolder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-courses", "files");

                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CourseFile.FileName)}";
                var fileFullPath = Path.Combine(fileFolder, fileName);

                using (var stream = new FileStream(fileFullPath, FileMode.Create))
                {
                    await model.CourseFile.CopyToAsync(stream);
                }

                filePath = "/uploads/teacher-courses/files/" + fileName;
            }

            var course = new TeacherCourse
            {
                Title = model.Title.Trim(),
                Category = model.Category?.Trim(),
                Level = model.Level?.Trim(),
                Description = model.Description?.Trim(),
                ThumbnailPath = thumbnailPath,
                FilePath = filePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };

            _context.TeacherCourses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course == null)
                return NotFound();

            var model = new TeacherCourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Category = course.Category,
                Level = course.Level,
                Description = course.Description,
                ExistingThumbnailPath = course.ThumbnailPath,
                ExistingFilePath = course.FilePath,
                IsArchived = course.IsArchived,
                CreatedAt = course.CreatedAt
            };

            return View("~/Views/Teacher/Courses/Edit.cshtml", model);
        }

        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeacherCourseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == model.Id && c.UserId == user.Id);

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.ExistingThumbnailPath = course.ThumbnailPath;
                model.ExistingFilePath = course.FilePath;
                return View("~/Views/Teacher/Courses/Edit.cshtml", model);
            }

            course.Title = model.Title.Trim();
            course.Category = model.Category?.Trim();
            course.Level = model.Level?.Trim();
            course.Description = model.Description?.Trim();

            if (model.Thumbnail != null && model.Thumbnail.Length > 0)
            {
                var imageFolder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-courses", "images");

                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Thumbnail.FileName)}";
                var imageFullPath = Path.Combine(imageFolder, imageName);

                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    await model.Thumbnail.CopyToAsync(stream);
                }

                course.ThumbnailPath = "/uploads/teacher-courses/images/" + imageName;
            }

            if (model.CourseFile != null && model.CourseFile.Length > 0)
            {
                var fileFolder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-courses", "files");

                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CourseFile.FileName)}";
                var fileFullPath = Path.Combine(fileFolder, fileName);

                using (var stream = new FileStream(fileFullPath, FileMode.Create))
                {
                    await model.CourseFile.CopyToAsync(stream);
                }

                course.FilePath = "/uploads/teacher-courses/files/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course != null)
            {
                course.IsArchived = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "TeacherArchive");
        }

        [HttpPost("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course != null)
            {
                course.IsArchived = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "TeacherArchive");
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course != null)
            {
                _context.TeacherCourses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}