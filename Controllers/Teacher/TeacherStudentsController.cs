using Acadimy.Data;
using Acadimy.Models;
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

        public TeacherStudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, int? courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var query = _context.TeacherEnrollments
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                .Where(e => e.TeacherCourse != null && e.TeacherCourse.UserId == user.Id && !e.TeacherCourse.IsArchived)
                .AsQueryable();

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
            {
                query = query.Where(e => e.TeacherCourseId == courseId.Value);
            }

            var students = await query
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
                .ToListAsync();

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

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var student = await _context.TeacherEnrollments
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                .Where(e => e.Id == id && e.TeacherCourse != null && e.TeacherCourse.UserId == user.Id)
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

            if (student == null)
                return NotFound();

            return View("~/Views/Teacher/Students/Details.cshtml", student);
        }
    }
}