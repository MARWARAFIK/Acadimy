using Acadimy.Data;
using Acadimy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Acadimy.ViewModels.Admin;
namespace Acadimy.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.StudentsCount = (await _userManager.GetUsersInRoleAsync("Étudiant")).Count;
            ViewBag.TeachersCount = (await _userManager.GetUsersInRoleAsync("Enseignant")).Count;
            ViewBag.CoursesCount = await _context.TeacherCourses.CountAsync();
            ViewBag.ProjectsCount = await _context.ProjectPosts.CountAsync();
            ViewBag.UsersCount = await _context.Users.CountAsync();

            return View();
        }

        public async Task<IActionResult> Students(string? search)
        {
            var students = await _userManager.GetUsersInRoleAsync("Étudiant");

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                students = students.Where(u =>
                    (u.FullName ?? "").ToLower().Contains(search) ||
                    (u.Email ?? "").ToLower().Contains(search) ||
                    (u.Filiere ?? "").ToLower().Contains(search) ||
                    (u.Niveau ?? "").ToLower().Contains(search)
                ).ToList();
            }

            ViewBag.Search = search;
            return View(students);
        }

        public async Task<IActionResult> Teachers(string? search)
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Enseignant");

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                teachers = teachers.Where(u =>
                    (u.FullName ?? "").ToLower().Contains(search) ||
                    (u.Email ?? "").ToLower().Contains(search) ||
                    (u.Grade ?? "").ToLower().Contains(search) ||
                    (u.Specialite ?? "").ToLower().Contains(search)
                ).ToList();
            }

            ViewBag.Search = search;
            return View(teachers);
        }

        public async Task<IActionResult> Courses(string? search)
        {
            var query = _context.TeacherCourses
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                query = query.Where(c =>
                    c.Title.ToLower().Contains(search) ||
                    (c.Category ?? "").ToLower().Contains(search) ||
                    (c.Level ?? "").ToLower().Contains(search) ||
                    (c.User != null && (c.User.FirstName + " " + c.User.LastName).ToLower().Contains(search))
                );
            }

            ViewBag.Search = search;
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Projects(string? search)
        {
            var query = _context.ProjectPosts
                .Include(p => p.User)
                .Include(p => p.Ratings)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                query = query.Where(p =>
                    p.Title.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    (p.User != null && (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(search))
                );
            }

            ViewBag.Search = search;
            return View(await query.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlockUser(string id, string returnAction)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null && user.Email != "admin@acadimy.com")
            {
                user.IsBlocked = !user.IsBlocked;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(returnAction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id, string returnAction)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null && user.Email != "admin@acadimy.com")
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction(returnAction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.ProjectPosts
                .Include(p => p.Comments)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project != null)
            {
                _context.ProjectComments.RemoveRange(project.Comments);
                _context.ProjectRatings.RemoveRange(project.Ratings);
                _context.ProjectPosts.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Projects");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course != null)
            {
                _context.TeacherCourses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Courses");
        }
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return RedirectToAction("Login", "Account");

            var model = new AdminSettingsViewModel
            {
                FirstName = admin.FirstName ?? "",
                LastName = admin.LastName ?? "",
                Email = admin.Email ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AdminSettingsViewModel model)
        {
            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            admin.FirstName = model.FirstName;
            admin.LastName = model.LastName;

            var updateResult = await _userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                !string.IsNullOrWhiteSpace(model.NewPassword) ||
                !string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(model.NewPassword) ||
                    string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Pour changer le mot de passe, remplissez les 3 champs.");
                    return View(model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(
                    admin,
                    model.CurrentPassword,
                    model.NewPassword
                );

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }
            }

            TempData["Success"] = "Paramètres mis à jour avec succès.";
            return RedirectToAction("Settings");
        }
    }
}