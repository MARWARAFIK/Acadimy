using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.Services;
using Acadimy.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    [Route("StudentAssignments")]
    public class StudentAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly NotificationService _notificationService;

        public StudentAssignmentsController(
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
            var student = await _userManager.GetUserAsync(User);
            if (student == null)
                return RedirectToAction("Login", "Account");

            var assignments = await _context.TeacherAssignments
                .Include(a => a.TeacherCourse)
                .Where(a =>
                    !a.IsArchived &&
                    _context.TeacherEnrollments.Any(e =>
                        e.StudentId == student.Id &&
                        e.TeacherCourseId == a.TeacherCourseId))
                .OrderBy(a => a.Deadline)
                .Select(a => new StudentAssignmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    CourseTitle = a.TeacherCourse != null ? a.TeacherCourse.Title : "",
                    FilePath = a.FilePath,

                    IsSubmitted = _context.TeacherAssignmentSubmissions
                        .Any(s => s.TeacherAssignmentId == a.Id && s.StudentId == student.Id),

                    Grade = _context.TeacherAssignmentSubmissions
                        .Where(s => s.TeacherAssignmentId == a.Id && s.StudentId == student.Id)
                        .Select(s => s.Grade)
                        .FirstOrDefault(),

                    TeacherFeedback = _context.TeacherAssignmentSubmissions
                        .Where(s => s.TeacherAssignmentId == a.Id && s.StudentId == student.Id)
                        .Select(s => s.TeacherFeedback)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return View("~/Views/Student/Assignments/Index.cshtml", assignments);
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _userManager.GetUserAsync(User);
            if (student == null)
                return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .Include(a => a.TeacherCourse)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    !a.IsArchived &&
                    _context.TeacherEnrollments.Any(e =>
                        e.StudentId == student.Id &&
                        e.TeacherCourseId == a.TeacherCourseId));

            if (assignment == null)
                return NotFound();

            var submission = await _context.TeacherAssignmentSubmissions
                .FirstOrDefaultAsync(s =>
                    s.TeacherAssignmentId == assignment.Id &&
                    s.StudentId == student.Id);

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                CourseTitle = assignment.TeacherCourse?.Title ?? "",
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                AssignmentFilePath = assignment.FilePath,
                ExistingSubmissionFile = submission?.FilePath,
                Comment = submission?.Comment,
                Grade = submission?.Grade,
                TeacherFeedback = submission?.TeacherFeedback
            };

            return View("~/Views/Student/Assignments/Details.cshtml", model);
        }

        [HttpPost("Submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitAssignmentViewModel model)
        {
            var student = await _userManager.GetUserAsync(User);
            if (student == null)
                return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .Include(a => a.TeacherCourse)
                .FirstOrDefaultAsync(a =>
                    a.Id == model.AssignmentId &&
                    !a.IsArchived &&
                    _context.TeacherEnrollments.Any(e =>
                        e.StudentId == student.Id &&
                        e.TeacherCourseId == a.TeacherCourseId));

            if (assignment == null)
                return NotFound();

            if (assignment.Deadline < DateTime.Now)
            {
                TempData["Error"] = "Deadline dépassée. Vous ne pouvez plus envoyer la solution.";
                return RedirectToAction(nameof(Details), new { id = model.AssignmentId });
            }

            var submission = await _context.TeacherAssignmentSubmissions
                .FirstOrDefaultAsync(s =>
                    s.TeacherAssignmentId == model.AssignmentId &&
                    s.StudentId == student.Id);

            string? filePath = submission?.FilePath;

            if (model.File != null && model.File.Length > 0)
            {
                filePath = await SaveSubmissionFile(model.File);
            }

            if (submission == null)
            {
                submission = new TeacherAssignmentSubmission
                {
                    TeacherAssignmentId = model.AssignmentId,
                    StudentId = student.Id,
                    FilePath = filePath,
                    Comment = model.Comment?.Trim(),
                    SubmittedAt = DateTime.Now
                };

                _context.TeacherAssignmentSubmissions.Add(submission);
            }
            else
            {
                submission.FilePath = filePath;
                submission.Comment = model.Comment?.Trim();
                submission.SubmittedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            if (assignment.TeacherCourse != null)
            {
                var teacher = await _userManager.FindByIdAsync(assignment.TeacherCourse.UserId);

                if (teacher != null && teacher.NotifyStudentSubmission)
                {
                    await _notificationService.SendAsync(
                        teacher.Id,
                        "Nouvelle submission",
                        $"{student.FullName} a envoyé une solution pour \"{assignment.Title}\".",
                        "Submission",
                        $"/TeacherAssignments/Submissions/{assignment.Id}"
                    );
                }
            }

            TempData["Success"] = "Solution envoyée avec succès.";
            return RedirectToAction(nameof(Details), new { id = model.AssignmentId });
        }

        private async Task<string> SaveSubmissionFile(IFormFile file)
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "student-submissions");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/student-submissions/" + fileName;
        }
    }
}