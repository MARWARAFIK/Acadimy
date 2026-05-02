using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.Services;
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
        private readonly NotificationService _notificationService;

        public TeacherCoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _notificationService = notificationService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

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
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View("~/Views/Teacher/Courses/Create.cshtml", model);

            var course = await BuildCourseFromModel(model, user.Id);

            _context.TeacherCourses.Add(course);
            await _context.SaveChangesAsync();

            await NotifyRelatedStudentsAboutCourse(course);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("CreateAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(TeacherCourseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Not authenticated" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Invalid data", errors });
            }

            var course = await BuildCourseFromModel(model, user.Id);

            _context.TeacherCourses.Add(course);
            await _context.SaveChangesAsync();

            await NotifyRelatedStudentsAboutCourse(course);

            return Json(new
            {
                success = true,
                course = new
                {
                    id = course.Id,
                    title = course.Title,
                    category = course.Category,
                    level = course.Level,
                    description = course.Description,
                    thumbnailPath = course.ThumbnailPath,
                    filePath = course.FilePath,
                    createdAt = course.CreatedAt.ToString("dd/MM/yyyy")
                }
            });
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course == null) return NotFound();

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

            ViewBag.Lessons = course.Lessons.OrderBy(l => l.Order).ToList();

            return View("~/Views/Teacher/Courses/Edit.cshtml", model);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherCourseViewModel model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course == null) return NotFound();

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
                course.ThumbnailPath = await SaveFile(model.Thumbnail, "uploads/teacher-courses/images");

            if (model.CourseFile != null && model.CourseFile.Length > 0)
                course.FilePath = await SaveFile(model.CourseFile, "uploads/teacher-courses/files");

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("AddLesson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(int courseId, string title, string? description)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.UserId == user.Id);

            if (course == null) return NotFound();

            if (string.IsNullOrWhiteSpace(title))
                return RedirectToAction(nameof(Edit), new { id = courseId });

            var nextOrder = await _context.CourseLessons
                .Where(l => l.TeacherCourseId == courseId)
                .CountAsync() + 1;

            var lesson = new CourseLesson
            {
                TeacherCourseId = courseId,
                Title = title.Trim(),
                Description = description?.Trim(),
                Order = nextOrder,
                CreatedAt = DateTime.Now
            };

            _context.CourseLessons.Add(lesson);
            await _context.SaveChangesAsync();

            var enrollments = await _context.TeacherEnrollments
                .Include(e => e.Student)
                .Where(e => e.TeacherCourseId == courseId)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                if (enrollment.Student != null && enrollment.Student.NotifyNewLesson)
                {
                    await _notificationService.SendAsync(
                        enrollment.StudentId,
                        "Nouvelle leçon ajoutée",
                        $"Une nouvelle leçon a été ajoutée au cours \"{course.Title}\".",
                        "Lesson",
                        $"/StudentCourses/Details/{course.Id}"
                    );
                }
            }

            return RedirectToAction(nameof(Edit), new { id = courseId });
        }

        [HttpPost("DeleteLesson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lesson = await _context.CourseLessons
                .Include(l => l.TeacherCourse)
                .FirstOrDefaultAsync(l =>
                    l.Id == lessonId &&
                    l.TeacherCourse != null &&
                    l.TeacherCourse.UserId == user.Id);

            if (lesson == null) return NotFound();

            var courseId = lesson.TeacherCourseId;

            _context.CourseLessons.Remove(lesson);
            await _context.SaveChangesAsync();

            await RecalculateAllEnrollmentsProgress(courseId);

            return RedirectToAction(nameof(Edit), new { id = courseId });
        }

        [HttpPost("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

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
            if (user == null) return RedirectToAction("Login", "Account");

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
            if (user == null) return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (course != null)
            {
                _context.TeacherCourses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<TeacherCourse> BuildCourseFromModel(TeacherCourseViewModel model, string userId)
        {
            string? thumbnailPath = null;
            string? filePath = null;

            if (model.Thumbnail != null && model.Thumbnail.Length > 0)
                thumbnailPath = await SaveFile(model.Thumbnail, "uploads/teacher-courses/images");

            if (model.CourseFile != null && model.CourseFile.Length > 0)
                filePath = await SaveFile(model.CourseFile, "uploads/teacher-courses/files");

            return new TeacherCourse
            {
                Title = model.Title.Trim(),
                Category = model.Category?.Trim(),
                Level = model.Level?.Trim(),
                Description = model.Description?.Trim(),
                ThumbnailPath = thumbnailPath,
                FilePath = filePath,
                UserId = userId,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };
        }

        private async Task<string> SaveFile(IFormFile file, string folderPath)
        {
            var folder = Path.Combine(_environment.WebRootPath, folderPath);
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/" + folderPath.Replace("\\", "/") + "/" + fileName;
        }

        private async Task NotifyRelatedStudentsAboutCourse(TeacherCourse course)
        {
            var students = await _userManager.GetUsersInRoleAsync("Étudiant");

            var relatedStudentIds = students
                .Where(s => s.NotifyNewCourse)
                .Where(s => IsCourseRelatedToStudent(course, s))
                .Select(s => s.Id)
                .ToList();

            await _notificationService.SendToManyAsync(
                relatedStudentIds,
                "Nouveau cours recommandé",
                $"Le cours \"{course.Title}\" correspond à votre profil.",
                "Course",
                $"/StudentCourses/Details/{course.Id}"
            );
        }

        private bool IsCourseRelatedToStudent(TeacherCourse course, ApplicationUser student)
        {
            var filiere = student.Filiere?.ToLower() ?? "";
            var niveau = student.Niveau?.ToLower() ?? "";
            var skill = student.Skill?.ToLower() ?? "";

            var title = course.Title.ToLower();
            var category = course.Category?.ToLower() ?? "";
            var level = course.Level?.ToLower() ?? "";
            var description = course.Description?.ToLower() ?? "";

            return
                (!string.IsNullOrWhiteSpace(filiere) &&
                 (title.Contains(filiere) || category.Contains(filiere) || description.Contains(filiere)))
                ||
                (!string.IsNullOrWhiteSpace(skill) &&
                 (title.Contains(skill) || category.Contains(skill) || description.Contains(skill)))
                ||
                (!string.IsNullOrWhiteSpace(niveau) &&
                 level.Contains(niveau));
        }

        private async Task RecalculateAllEnrollmentsProgress(int courseId)
        {
            var enrollments = await _context.TeacherEnrollments
                .Where(e => e.TeacherCourseId == courseId)
                .ToListAsync();

            var totalLessons = await _context.CourseLessons
                .CountAsync(l => l.TeacherCourseId == courseId);

            foreach (var enrollment in enrollments)
            {
                var completedLessons = await _context.StudentLessonProgresses
                    .CountAsync(p =>
                        p.StudentId == enrollment.StudentId &&
                        p.CourseLesson != null &&
                        p.CourseLesson.TeacherCourseId == courseId);

                var progress = totalLessons == 0 ? 0 : completedLessons * 100 / totalLessons;

                enrollment.ProgressPercent = progress;
                enrollment.IsCompleted = progress >= 100;

                if (progress >= 100)
                    enrollment.Status = "Completed";
                else if (enrollment.Status != "Paused")
                    enrollment.Status = "Active";
            }

            await _context.SaveChangesAsync();
        }
    }
}