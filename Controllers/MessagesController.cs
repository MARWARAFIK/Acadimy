using Acadimy.Data;
using Acadimy.Hubs;
using Acadimy.Models;
using Acadimy.Models.Messaging;
using Acadimy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly NotificationService _notificationService;
        private readonly IHubContext<ChatHub> _hub;
        private readonly OnlineUserTracker _tracker;

        public MessagesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            NotificationService notificationService,
            IHubContext<ChatHub> hub,
            OnlineUserTracker tracker)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _notificationService = notificationService;
            _hub = hub;
            _tracker = tracker;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var threads = await _context.MessageThreads
                .Where(t =>
                    (t.User1Id == user.Id || t.User2Id == user.Id) &&
                    t.Messages.Any())
                .Include(t => t.Messages)
                .OrderByDescending(t => t.Messages.Max(m => m.SentAt))
                .ToListAsync();

            var courseGroups = User.IsInRole("Enseignant")
                ? await _context.TeacherCourses
                    .Where(c => c.UserId == user.Id && !c.IsArchived)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync()
                : await _context.TeacherEnrollments
                    .Include(e => e.TeacherCourse)
                    .Where(e => e.StudentId == user.Id && e.TeacherCourse != null && !e.TeacherCourse.IsArchived)
                    .Select(e => e.TeacherCourse!)
                    .ToListAsync();

            ViewBag.CourseGroups = courseGroups;

            return View(threads);
        }

        public async Task<IActionResult> Chat(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (otherUser == null) return NotFound();

            var thread = await _context.MessageThreads
                .Include(t => t.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(t =>
                    (t.User1Id == currentUser.Id && t.User2Id == userId) ||
                    (t.User2Id == currentUser.Id && t.User1Id == userId));

            if (thread == null)
            {
                thread = new MessageThread
                {
                    User1Id = currentUser.Id,
                    User2Id = userId,
                    CreatedAt = DateTime.Now
                };

                _context.MessageThreads.Add(thread);
                await _context.SaveChangesAsync();
            }

            foreach (var msg in thread.Messages.Where(m => m.SenderId != currentUser.Id && !m.IsRead))
                msg.IsRead = true;

            await _context.SaveChangesAsync();

            ViewBag.ThreadId = thread.Id;
            ViewBag.OtherUser = otherUser;
            ViewBag.CurrentUserId = currentUser.Id;

            return View(thread.Messages.OrderBy(m => m.SentAt).ToList());
        }

        [HttpGet]
        public IActionResult IsOnline(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Json(false);

            return Json(_tracker.IsOnline(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAjax(int threadId, string? content, IFormFile? attachment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Not authenticated" });

            var thread = await _context.MessageThreads
                .FirstOrDefaultAsync(t =>
                    t.Id == threadId &&
                    (t.User1Id == user.Id || t.User2Id == user.Id));

            if (thread == null)
                return Json(new { success = false, message = "Thread not found" });

            var saved = await SaveAttachmentAsync(attachment);

            if (string.IsNullOrWhiteSpace(content) && saved.Path == null)
                return Json(new { success = false, message = "Message vide" });

            var message = new Message
            {
                ThreadId = threadId,
                SenderId = user.Id,
                Content = content?.Trim() ?? "",
                AttachmentPath = saved.Path,
                AttachmentFileName = saved.FileName,
                AttachmentContentType = saved.ContentType,
                IsVoice = saved.ContentType?.StartsWith("audio/") == true,
                SentAt = DateTime.Now,
                IsRead = false,
                IsDeleted = false,
                IsEdited = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var receiverId = thread.User1Id == user.Id ? thread.User2Id : thread.User1Id;
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverId);

            if (receiver != null && receiver.NotifyMessages)
            {
                await _notificationService.SendAsync(
                    receiver.Id,
                    "Nouveau message",
                    $"{user.FullName} vous a envoyé un message.",
                    "Message",
                    $"/Messages/Chat?userId={user.Id}"
                );
            }

            var result = new
            {
                success = true,
                messageId = message.Id,
                senderId = user.Id,
                fullName = user.FullName,
                content = message.Content,
                sentAt = message.SentAt.ToString("dd/MM HH:mm"),
                attachmentPath = message.AttachmentPath,
                attachmentFileName = message.AttachmentFileName,
                attachmentContentType = message.AttachmentContentType,
                isVoice = message.IsVoice,
                isRead = message.IsRead,
                isEdited = message.IsEdited,
                isDeleted = message.IsDeleted
            };

            await _hub.Clients.Group(threadId.ToString())
                .SendAsync("ReceiveMessage", result);

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessageAjax(int messageId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Not authenticated" });

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == user.Id);

            if (message == null)
                return Json(new { success = false, message = "Message not found" });

            if (message.IsDeleted)
                return Json(new { success = false, message = "Message already deleted" });

            if (string.IsNullOrWhiteSpace(content))
                return Json(new { success = false, message = "Message vide" });

            message.Content = content.Trim();
            message.IsEdited = true;
            message.EditedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await _hub.Clients.Group(message.ThreadId.ToString())
                .SendAsync("MessageUpdated", message.Id, message.Content);

            return Json(new
            {
                success = true,
                messageId = message.Id,
                content = message.Content
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessageAjax(int messageId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Not authenticated" });

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == user.Id);

            if (message == null)
                return Json(new { success = false, message = "Message not found" });

            message.IsDeleted = true;
            message.IsEdited = false;
            message.Content = "";
            message.AttachmentPath = null;
            message.AttachmentFileName = null;
            message.AttachmentContentType = null;
            message.IsVoice = false;

            await _context.SaveChangesAsync();

            await _hub.Clients.Group(message.ThreadId.ToString())
                .SendAsync("MessageDeleted", message.Id);

            return Json(new
            {
                success = true,
                messageId = message.Id
            });
        }

        [HttpGet]
        public async Task<IActionResult> CourseGroup(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var hasAccess = await UserHasCourseAccess(user.Id, courseId);
            if (!hasAccess) return Forbid();

            var course = await _context.TeacherCourses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            var messages = await _context.CourseGroupMessages
                .Include(m => m.Sender)
                .Where(m => m.TeacherCourseId == courseId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.Course = course;

            return View("~/Views/Messages/CourseGroup.cshtml", messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCourseGroup(int courseId, string? content, IFormFile? attachment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var hasAccess = await UserHasCourseAccess(user.Id, courseId);
            if (!hasAccess) return Forbid();

            var saved = await SaveAttachmentAsync(attachment);

            if (string.IsNullOrWhiteSpace(content) && saved.Path == null)
                return RedirectToAction(nameof(CourseGroup), new { courseId });

            var message = new CourseGroupMessage
            {
                TeacherCourseId = courseId,
                SenderId = user.Id,
                Content = content?.Trim() ?? "",
                AttachmentPath = saved.Path,
                AttachmentFileName = saved.FileName,
                AttachmentContentType = saved.ContentType,
                IsVoice = saved.ContentType?.StartsWith("audio/") == true,
                SentAt = DateTime.Now
            };

            _context.CourseGroupMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CourseGroup), new { courseId });
        }

        private async Task<bool> UserHasCourseAccess(string userId, int courseId)
        {
            var isTeacherOwner = await _context.TeacherCourses
                .AnyAsync(c => c.Id == courseId && c.UserId == userId && !c.IsArchived);

            if (isTeacherOwner)
                return true;

            return await _context.TeacherEnrollments
                .AnyAsync(e =>
                    e.TeacherCourseId == courseId &&
                    e.StudentId == userId &&
                    e.TeacherCourse != null &&
                    !e.TeacherCourse.IsArchived);
        }

        private async Task<(string? Path, string? FileName, string? ContentType)> SaveAttachmentAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (null, null, null);

            var folder = Path.Combine(_environment.WebRootPath, "uploads", "messages");
            Directory.CreateDirectory(folder);

            var safeFileName = Path.GetFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(safeFileName)}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return ($"/uploads/messages/{fileName}", safeFileName, file.ContentType);
        }
    }
}