using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    [Route("StudentCourses")]
    public class StudentCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public StudentCoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search)
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var query = _context.TeacherCourses
                .Include(c => c.User)
                .Include(c => c.Enrollments)
                .Where(c => c.IsArchived == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();

                query = query.Where(c =>
                    c.Title.ToLower().Contains(s) ||
                    (c.Category != null && c.Category.ToLower().Contains(s)) ||
                    (c.Level != null && c.Level.ToLower().Contains(s)) ||
                    (c.Description != null && c.Description.ToLower().Contains(s)) ||
                    (c.User != null &&
                        (
                            (c.User.FirstName != null && c.User.FirstName.ToLower().Contains(s)) ||
                            (c.User.LastName != null && c.User.LastName.ToLower().Contains(s)) ||
                            (c.User.Specialite != null && c.User.Specialite.ToLower().Contains(s))
                        )
                    )
                );
            }

            var courses = await query.ToListAsync();

            courses = courses
                .OrderByDescending(c => GetScore(c, student))
                .ThenByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.Search = search;
            ViewBag.StudentId = student.Id;

            return View("~/Views/Student/Courses/Index.cshtml", courses);
        }

        [HttpGet("MyCourses")]
        public async Task<IActionResult> MyCourses()
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var courses = await _context.TeacherEnrollments
                .Include(e => e.TeacherCourse)
                    .ThenInclude(c => c!.User)
                .Where(e =>
                    e.StudentId == student.Id &&
                    e.TeacherCourse != null &&
                    e.TeacherCourse.IsArchived == false)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return View("~/Views/Student/Courses/MyCourses.cshtml", courses);
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .Include(c => c.User)
                .Include(c => c.Enrollments)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsArchived == false);

            if (course == null)
                return NotFound();

            var enrollment = await _context.TeacherEnrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentId == student.Id &&
                    e.TeacherCourseId == id);

            var completedLessonIds = await _context.StudentLessonProgresses
                .Where(p =>
                    p.StudentId == student.Id &&
                    p.CourseLesson != null &&
                    p.CourseLesson.TeacherCourseId == id)
                .Select(p => p.CourseLessonId)
                .ToListAsync();

            ViewBag.IsJoined = enrollment != null;
            ViewBag.Progress = enrollment?.ProgressPercent ?? 0;
            ViewBag.Status = enrollment?.Status ?? "Not joined";
            ViewBag.CompletedLessonIds = completedLessonIds;

            return View("~/Views/Student/Courses/Details.cshtml", course);
        }
        [HttpPost("JoinAjax/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinAjax(int id)
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return Json(new { success = false, message = "Not authenticated" });

            var course = await _context.TeacherCourses
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsArchived);

            if (course == null)
                return Json(new { success = false, message = "Course not found" });

            var exists = await _context.TeacherEnrollments
                .AnyAsync(e => e.TeacherCourseId == id && e.StudentId == student.Id);

            if (!exists)
            {
                _context.TeacherEnrollments.Add(new TeacherEnrollment
                {
                    TeacherCourseId = id,
                    StudentId = student.Id,
                    EnrolledAt = DateTime.Now,
                    ProgressPercent = 0,
                    IsCompleted = false,
                    Status = "Active"
                });

                await _context.SaveChangesAsync();

                if (course.User != null && course.User.NotifyCourseActivity)
                {
                    await _notificationService.SendAsync(
                        course.User.Id,
                        "Nouvel étudiant inscrit",
                        $"{student.FullName} a rejoint votre cours \"{course.Title}\".",
                        "Course",
                        $"/TeacherStudents"
                    );
                }
            }

            return Json(new
            {
                success = true,
                progress = 0,
                status = "Active",
                groupUrl = Url.Action("CourseGroup", "Messages", new { courseId = id })
            });
        }

        [HttpPost("CompleteLessonAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLessonAjax(int lessonId)
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return Json(new { success = false, message = "Not authenticated" });

            var lesson = await _context.CourseLessons
                .Include(l => l.TeacherCourse)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return Json(new { success = false, message = "Lesson not found" });

            var enrollment = await _context.TeacherEnrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentId == student.Id &&
                    e.TeacherCourseId == lesson.TeacherCourseId);

            if (enrollment == null)
                return Json(new { success = false, message = "Vous devez vous inscrire d'abord." });

            if (enrollment.Status == "Paused")
                return Json(new { success = false, message = "Ce cours est en pause." });

            var alreadyDone = await _context.StudentLessonProgresses
                .AnyAsync(p => p.StudentId == student.Id && p.CourseLessonId == lessonId);

            if (!alreadyDone)
            {
                _context.StudentLessonProgresses.Add(new StudentLessonProgress
                {
                    StudentId = student.Id,
                    CourseLessonId = lessonId,
                    CompletedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            await UpdateEnrollmentProgress(student.Id, lesson.TeacherCourseId);

            var updatedEnrollment = await _context.TeacherEnrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentId == student.Id &&
                    e.TeacherCourseId == lesson.TeacherCourseId);

            return Json(new
            {
                success = true,
                lessonId = lesson.Id,
                progress = updatedEnrollment?.ProgressPercent ?? 0,
                status = updatedEnrollment?.Status ?? "Active"
            });
        }

        [HttpPost("Join/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var course = await _context.TeacherCourses
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsArchived == false);

            if (course == null)
                return NotFound();

            var exists = await _context.TeacherEnrollments
                .AnyAsync(e =>
                    e.TeacherCourseId == id &&
                    e.StudentId == student.Id);

            if (!exists)
            {
                _context.TeacherEnrollments.Add(new TeacherEnrollment
                {
                    TeacherCourseId = id,
                    StudentId = student.Id,
                    EnrolledAt = DateTime.Now,
                    ProgressPercent = 0,
                    IsCompleted = false,
                    Status = "Active"
                });

                await _context.SaveChangesAsync();

                if (course.User != null && course.User.NotifyCourseActivity)
                {
                    await _notificationService.SendAsync(
                        course.User.Id,
                        "Nouvel étudiant inscrit",
                        $"{student.FullName} a rejoint votre cours \"{course.Title}\".",
                        "Course",
                        $"/TeacherCourses/Edit/{course.Id}"
                    );
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("CompleteLesson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var student = await _userManager.GetUserAsync(User);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var lesson = await _context.CourseLessons
                .Include(l => l.TeacherCourse)
                    .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return NotFound();

            var enrollment = await _context.TeacherEnrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentId == student.Id &&
                    e.TeacherCourseId == lesson.TeacherCourseId);

            if (enrollment == null)
                return RedirectToAction(nameof(Details), new { id = lesson.TeacherCourseId });

            var alreadyDone = await _context.StudentLessonProgresses
                .AnyAsync(p =>
                    p.StudentId == student.Id &&
                    p.CourseLessonId == lessonId);

            if (!alreadyDone)
            {
                _context.StudentLessonProgresses.Add(new StudentLessonProgress
                {
                    StudentId = student.Id,
                    CourseLessonId = lessonId,
                    CompletedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();

                if (lesson.TeacherCourse?.User != null &&
                    lesson.TeacherCourse.User.NotifyCourseActivity)
                {
                    await _notificationService.SendAsync(
                        lesson.TeacherCourse.User.Id,
                        "Leçon complétée",
                        $"{student.FullName} a complété une leçon dans \"{lesson.TeacherCourse.Title}\".",
                        "Course",
                        $"/TeacherCourses/Edit/{lesson.TeacherCourseId}"
                    );
                }
            }

            await UpdateEnrollmentProgress(student.Id, lesson.TeacherCourseId);

            return RedirectToAction(nameof(Details), new { id = lesson.TeacherCourseId });
        }

        private async Task UpdateEnrollmentProgress(string studentId, int courseId)
        {
            var totalLessons = await _context.CourseLessons
                .CountAsync(l => l.TeacherCourseId == courseId);

            var completedLessons = await _context.StudentLessonProgresses
                .CountAsync(p =>
                    p.StudentId == studentId &&
                    p.CourseLesson != null &&
                    p.CourseLesson.TeacherCourseId == courseId);

            var progress = totalLessons == 0 ? 0 : completedLessons * 100 / totalLessons;

            var enrollment = await _context.TeacherEnrollments
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.TeacherCourseId == courseId);

            if (enrollment == null)
                return;

            var oldProgress = enrollment.ProgressPercent;
            var oldStatus = enrollment.Status;

            enrollment.ProgressPercent = progress;

            if (progress >= 100)
            {
                enrollment.ProgressPercent = 100;
                enrollment.Status = "Completed";
                enrollment.IsCompleted = true;
            }
            else
            {
                enrollment.IsCompleted = false;

                if (enrollment.Status != "Paused")
                    enrollment.Status = "Active";
            }

            await _context.SaveChangesAsync();

            if (enrollment.Student != null &&
                enrollment.Student.NotifyApplicationStatus &&
                (oldProgress != enrollment.ProgressPercent || oldStatus != enrollment.Status))
            {
                await _notificationService.SendAsync(
                    enrollment.StudentId,
                    "Progression du cours mise à jour",
                    $"Votre progression dans \"{enrollment.TeacherCourse?.Title}\" est maintenant {enrollment.ProgressPercent}% ({enrollment.Status}).",
                    "CourseProgress",
                    $"/StudentCourses/Details/{courseId}"
                );
            }
        }

        private int GetScore(TeacherCourse course, ApplicationUser student)
        {
            var score = 0;

            var title = course.Title.ToLower();
            var category = course.Category?.ToLower() ?? "";
            var level = course.Level?.ToLower() ?? "";
            var description = course.Description?.ToLower() ?? "";

            var filiere = student.Filiere?.ToLower() ?? "";
            var niveau = student.Niveau?.ToLower() ?? "";
            var skill = student.Skill?.ToLower() ?? "";

            if (!string.IsNullOrWhiteSpace(filiere) &&
                (title.Contains(filiere) || category.Contains(filiere) || description.Contains(filiere)))
            {
                score += 5;
            }

            if (!string.IsNullOrWhiteSpace(skill) &&
                (title.Contains(skill) || category.Contains(skill) || description.Contains(skill)))
            {
                score += 4;
            }

            if (!string.IsNullOrWhiteSpace(niveau) && level.Contains(niveau))
            {
                score += 3;
            }

            return score;
        }
    }
}