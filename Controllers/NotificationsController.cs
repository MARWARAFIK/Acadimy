using Acadimy.Data;
using Acadimy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers
{
    [Authorize]
    [Route("Notifications")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("~/Views/Notifications/Index.cshtml", notifications);
        }

        [HttpGet("Read/{id:int}")]
        public async Task<IActionResult> Read(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notification == null)
                return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            if (string.IsNullOrWhiteSpace(notification.Link))
                return RedirectToAction(nameof(Index));

            var link = notification.Link;

            if (link.StartsWith("/StudentCourses/Details/"))
            {
                var idText = link.Replace("/StudentCourses/Details/", "");

                if (int.TryParse(idText, out int courseId))
                {
                    var courseExists = await _context.TeacherCourses
                        .AnyAsync(c => c.Id == courseId && !c.IsArchived);

                    if (!courseExists)
                    {
                        TempData["NotificationError"] = "Ce cours n'est plus disponible.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            if (link.StartsWith("/TeacherCourses/Edit/"))
            {
                var idText = link.Replace("/TeacherCourses/Edit/", "");

                if (int.TryParse(idText, out int courseId))
                {
                    var courseExists = await _context.TeacherCourses
                        .AnyAsync(c => c.Id == courseId && !c.IsArchived);

                    if (!courseExists)
                    {
                        TempData["NotificationError"] = "Ce cours n'est plus disponible.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            if (link.StartsWith("/StudentAssignments/Details/"))
            {
                var idText = link.Replace("/StudentAssignments/Details/", "");

                if (int.TryParse(idText, out int assignmentId))
                {
                    var assignmentExists = await _context.TeacherAssignments
                        .AnyAsync(a => a.Id == assignmentId && !a.IsArchived);

                    if (!assignmentExists)
                    {
                        TempData["NotificationError"] = "Cet assignment n'est plus disponible.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            if (link.StartsWith("/TeacherAssignments/Submissions/"))
            {
                var idText = link.Replace("/TeacherAssignments/Submissions/", "");

                if (int.TryParse(idText, out int assignmentId))
                {
                    var assignmentExists = await _context.TeacherAssignments
                        .AnyAsync(a => a.Id == assignmentId && !a.IsArchived);

                    if (!assignmentExists)
                    {
                        TempData["NotificationError"] = "Cet assignment n'est plus disponible.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return Redirect(link);
        }

        [HttpPost("MarkAllAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var item in notifications)
                item.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}