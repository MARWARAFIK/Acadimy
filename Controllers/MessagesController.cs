using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📥 Liste conversations
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var threads = await _context.MessageThreads
                .Where(t => t.User1Id == user.Id || t.User2Id == user.Id)
                .Include(t => t.Messages)
                .OrderByDescending(t => t.Messages.Max(m => m.SentAt))
                .ToListAsync();

            return View(threads);
        }

        // 💬 Chat avec user
        public async Task<IActionResult> Chat(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var thread = await _context.MessageThreads
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t =>
                    (t.User1Id == currentUser.Id && t.User2Id == userId) ||
                    (t.User2Id == currentUser.Id && t.User1Id == userId));

            if (thread == null)
            {
                thread = new MessageThread
                {
                    User1Id = currentUser.Id,
                    User2Id = userId
                };

                _context.MessageThreads.Add(thread);
                await _context.SaveChangesAsync();
            }

            // 🔴 mark messages as read
            var unread = thread.Messages
                .Where(m => m.SenderId != currentUser.Id && !m.IsRead);

            foreach (var msg in unread)
                msg.IsRead = true;

            await _context.SaveChangesAsync();

            ViewBag.ThreadId = thread.Id;

            return View(thread.Messages.OrderBy(m => m.SentAt).ToList());
        }

        // 📤 Send message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int threadId, string content)
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrWhiteSpace(content))
                return Redirect(Request.Headers["Referer"].ToString());

            var message = new Message
            {
                ThreadId = threadId,
                SenderId = user.Id,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}