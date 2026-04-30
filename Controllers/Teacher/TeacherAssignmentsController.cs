using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.Services;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    [Route("TeacherAssignments")]
    public class TeacherAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly NotificationService _notificationService;

        public TeacherAssignmentsController(
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
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var assignments = await _context.TeacherAssignments
                .Include(a => a.TeacherCourse)
                .Include(a => a.Submissions)
                .Where(a => a.UserId == user.Id && !a.IsArchived)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new TeacherAssignmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    TeacherCourseId = a.TeacherCourseId,
                    CourseTitle = a.TeacherCourse != null ? a.TeacherCourse.Title : "",
                    ExistingFilePath = a.FilePath,
                    CreatedAt = a.CreatedAt,
                    SubmissionsCount = a.Submissions.Count
                })
                .ToListAsync();

            return View("~/Views/Teacher/Assignments/Index.cshtml", assignments);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            await LoadCourses(user.Id);

            return View("~/Views/Teacher/Assignments/Create.cshtml",
                new TeacherAssignmentViewModel
                {
                    Deadline = DateTime.Now.AddDays(7)
                });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherAssignmentViewModel model)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            if (!await CourseBelongsToTeacher(model.TeacherCourseId, user.Id))
                ModelState.AddModelError("TeacherCourseId", "Veuillez choisir un cours valide.");

            if (!ModelState.IsValid)
            {
                await LoadCourses(user.Id);
                return View("~/Views/Teacher/Assignments/Create.cshtml", model);
            }

            string? filePath = null;

            if (model.AssignmentFile != null && model.AssignmentFile.Length > 0)
                filePath = await SaveAssignmentFile(model.AssignmentFile);

            var assignment = new TeacherAssignment
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                Deadline = model.Deadline,
                FilePath = filePath,
                CreatedAt = DateTime.Now,
                IsArchived = false,
                UserId = user.Id,
                TeacherCourseId = model.TeacherCourseId
            };

            _context.TeacherAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .Include(a => a.TeacherCourse)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (assignment == null) return NotFound();

            await LoadCourses(user.Id);

            var model = new TeacherAssignmentViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                TeacherCourseId = assignment.TeacherCourseId,
                CourseTitle = assignment.TeacherCourse?.Title,
                ExistingFilePath = assignment.FilePath,
                CreatedAt = assignment.CreatedAt
            };

            return View("~/Views/Teacher/Assignments/Edit.cshtml", model);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherAssignmentViewModel model)
        {
            if (id != model.Id) return NotFound();

            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (assignment == null) return NotFound();

            if (!await CourseBelongsToTeacher(model.TeacherCourseId, user.Id))
                ModelState.AddModelError("TeacherCourseId", "Veuillez choisir un cours valide.");

            if (!ModelState.IsValid)
            {
                model.ExistingFilePath = assignment.FilePath;
                await LoadCourses(user.Id);
                return View("~/Views/Teacher/Assignments/Edit.cshtml", model);
            }

            assignment.Title = model.Title.Trim();
            assignment.Description = model.Description?.Trim();
            assignment.Deadline = model.Deadline;
            assignment.TeacherCourseId = model.TeacherCourseId;

            if (model.AssignmentFile != null && model.AssignmentFile.Length > 0)
                assignment.FilePath = await SaveAssignmentFile(model.AssignmentFile);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (assignment == null) return NotFound();

            assignment.IsArchived = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment archivé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (assignment == null) return NotFound();

            _context.TeacherAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Submissions/{id:int}")]
        public async Task<IActionResult> Submissions(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments
                .Include(a => a.TeacherCourse)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (assignment == null) return NotFound();

            var submissions = await _context.TeacherAssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.TeacherCourse)
                .Where(s => s.TeacherAssignmentId == id)
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new TeacherAssignmentSubmissionViewModel
                {
                    SubmissionId = s.Id,
                    AssignmentId = s.TeacherAssignmentId,
                    AssignmentTitle = s.TeacherAssignment != null ? s.TeacherAssignment.Title : "",
                    CourseTitle = s.TeacherAssignment != null && s.TeacherAssignment.TeacherCourse != null
                        ? s.TeacherAssignment.TeacherCourse.Title
                        : "",
                    StudentId = s.StudentId,
                    StudentName = s.Student != null ? s.Student.FullName : "",
                    StudentEmail = s.Student != null ? s.Student.Email : "",
                    ProfileImagePath = s.Student != null ? s.Student.ProfileImagePath : null,
                    FilePath = s.FilePath,
                    Comment = s.Comment,
                    SubmittedAt = s.SubmittedAt,
                    Grade = s.Grade,
                    TeacherFeedback = s.TeacherFeedback
                })
                .ToListAsync();

            ViewBag.AssignmentTitle = assignment.Title;
            ViewBag.CourseTitle = assignment.TeacherCourse?.Title;
            ViewBag.AssignmentId = assignment.Id;

            return View("~/Views/Teacher/Assignments/Submissions.cshtml", submissions);
        }

        [HttpPost("GradeSubmission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(
            int submissionId,
            decimal? grade,
            string? teacherFeedback)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            var submission = await _context.TeacherAssignmentSubmissions
                .Include(s => s.TeacherAssignment)
                .FirstOrDefaultAsync(s =>
                    s.Id == submissionId &&
                    s.TeacherAssignment != null &&
                    s.TeacherAssignment.UserId == user.Id);

            if (submission == null) return NotFound();

            if (grade.HasValue && (grade.Value < 0 || grade.Value > 20))
            {
                TempData["Error"] = "La note doit être entre 0 et 20.";
                return RedirectToAction(nameof(Submissions), new { id = submission.TeacherAssignmentId });
            }

            submission.Grade = grade;
            submission.TeacherFeedback = teacherFeedback?.Trim();

            await _context.SaveChangesAsync();

            var student = await _userManager.FindByIdAsync(submission.StudentId);

            if (student != null && student.NotifyApplicationStatus)
            {
                await _notificationService.SendAsync(
                    student.Id,
                    "Assignment corrigé",
                    $"Votre assignment \"{submission.TeacherAssignment?.Title}\" a été corrigé. Note: {(submission.Grade.HasValue ? submission.Grade + "/20" : "non définie")}.",
                    "Grade",
                    $"/StudentAssignments/Details/{submission.TeacherAssignmentId}"
                );
            }

            TempData["Success"] = "Note et feedback enregistrés avec succès.";
            return RedirectToAction(nameof(Submissions), new { id = submission.TeacherAssignmentId });
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        {
            return await _userManager.GetUserAsync(User);
        }

        private async Task<bool> CourseBelongsToTeacher(int courseId, string userId)
        {
            return await _context.TeacherCourses
                .AnyAsync(c => c.Id == courseId && c.UserId == userId && !c.IsArchived);
        }

        private async Task LoadCourses(string userId)
        {
            ViewBag.Courses = await _context.TeacherCourses
                .Where(c => c.UserId == userId && !c.IsArchived)
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                })
                .ToListAsync();
        }

        private async Task<string> SaveAssignmentFile(IFormFile file)
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-assignments");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/teacher-assignments/" + fileName;
        }
    }
}