using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Services;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    [Route("TeacherStudents")]
    public class TeacherStudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public TeacherStudentsController(
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
        public async Task<IActionResult> Index(string? search, int? courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _context.TeacherEnrollments
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                .Where(e => e.TeacherCourse != null
                            && e.TeacherCourse.UserId == user.Id
                            && !e.TeacherCourse.IsArchived);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(e =>
                    (e.Student != null && e.Student.FirstName != null && e.Student.FirstName.Contains(search)) ||
                    (e.Student != null && e.Student.LastName != null && e.Student.LastName.Contains(search)) ||
                    (e.Student != null && e.Student.Email != null && e.Student.Email.Contains(search)) ||
                    (e.TeacherCourse != null && e.TeacherCourse.Title.Contains(search)));
            }

            if (courseId.HasValue)
                query = query.Where(e => e.TeacherCourseId == courseId.Value);

            var enrollments = await query.ToListAsync();

            foreach (var e in enrollments)
            {
                if (e.ProgressPercent >= 100)
                {
                    e.ProgressPercent = 100;
                    e.IsCompleted = true;
                    e.Status = "Completed";
                }
                else if (e.Status != "Paused")
                {
                    e.IsCompleted = false;
                    e.Status = "Active";
                }
            }

            await _context.SaveChangesAsync();

            var students = enrollments
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new TeacherStudentViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    FullName = e.Student != null ? e.Student.FullName : "",
                    Email = e.Student != null ? e.Student.Email : "",
                    ProfileImagePath = e.Student != null ? e.Student.ProfileImagePath : null,
                    TeacherCourseId = e.TeacherCourseId,
                    CourseTitle = e.TeacherCourse != null ? e.TeacherCourse.Title : "",
                    Level = e.TeacherCourse != null ? e.TeacherCourse.Level : "",
                    EnrolledAt = e.EnrolledAt,
                    ProgressPercent = e.ProgressPercent,
                    Status = e.Status
                })
                .ToList();

            var teacherCourses = await _context.TeacherCourses
                .Where(c => c.UserId == user.Id && !c.IsArchived)
                .OrderBy(c => c.Title)
                .Select(c => new CourseFilterItem
                {
                    Id = c.Id,
                    Title = c.Title
                })
                .ToListAsync();

            var model = new TeacherStudentsPageViewModel
            {
                Students = students,
                Search = search,
                CourseId = courseId,
                Courses = teacherCourses,
                TotalStudents = students.Select(s => s.StudentId).Distinct().Count(),
                TotalCourses = teacherCourses.Count,
                NewStudentsCount = students.Count(s => s.EnrolledAt >= DateTime.Now.AddDays(-7)),
                CompletedCount = students.Count(s => s.ProgressPercent >= 100 || s.Status == "Completed")
            };

            return View("~/Views/Teacher/Students/Index.cshtml", model);
        }

        [HttpPost("TogglePauseAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePauseAjax(int enrollmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Not authenticated" });

            var enrollment = await _context.TeacherEnrollments
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                .FirstOrDefaultAsync(e =>
                    e.Id == enrollmentId &&
                    e.TeacherCourse != null &&
                    e.TeacherCourse.UserId == user.Id);

            if (enrollment == null)
                return Json(new { success = false, message = "Enrollment not found" });

            if (enrollment.ProgressPercent >= 100)
            {
                enrollment.Status = "Completed";
                enrollment.IsCompleted = true;
            }
            else
            {
                enrollment.Status = enrollment.Status == "Paused" ? "Active" : "Paused";
                enrollment.IsCompleted = false;
            }

            await _context.SaveChangesAsync();

            if (enrollment.Student != null && enrollment.Student.NotifyApplicationStatus)
            {
                await _notificationService.SendAsync(
                    enrollment.StudentId,
                    "Statut du cours mis à jour",
                    $"Votre statut dans le cours \"{enrollment.TeacherCourse?.Title}\" est maintenant {enrollment.Status}.",
                    "CourseStatus",
                    $"/StudentCourses/Details/{enrollment.TeacherCourseId}"
                );
            }

            return Json(new
            {
                success = true,
                status = enrollment.Status,
                isCompleted = enrollment.Status == "Completed"
            });
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _context.TeacherEnrollments
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                .Where(e => e.Id == id
                            && e.TeacherCourse != null
                            && e.TeacherCourse.UserId == user.Id)
                .Select(e => new TeacherStudentViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    FullName = e.Student != null ? e.Student.FullName : "",
                    Email = e.Student != null ? e.Student.Email : "",
                    ProfileImagePath = e.Student != null ? e.Student.ProfileImagePath : null,
                    TeacherCourseId = e.TeacherCourseId,
                    CourseTitle = e.TeacherCourse != null ? e.TeacherCourse.Title : "",
                    Level = e.TeacherCourse != null ? e.TeacherCourse.Level : "",
                    EnrolledAt = e.EnrolledAt,
                    ProgressPercent = e.ProgressPercent,
                    Status = e.Status
                })
                .FirstOrDefaultAsync();

            if (student == null) return NotFound();

            return View("~/Views/Teacher/Students/Details.cshtml", student);
        }
    }
}