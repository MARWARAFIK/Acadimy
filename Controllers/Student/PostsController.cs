using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Student;
using Acadimy.Models.Community;
using Acadimy.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // ================= CREATE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Content) &&
                (model.Image == null || model.Image.Length == 0))
                return RedirectToAction("Index", "Profile");

            string? imagePath = null;

            if (model.Image != null && model.Image.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "uploads/posts");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.Image.FileName);
                var filePath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);

                imagePath = "/uploads/posts/" + fileName;
            }

            var post = new StudentPost
            {
                Content = model.Content ?? "",
                ImagePath = imagePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };

            _context.StudentPosts.Add(post);
            await _context.SaveChangesAsync();

            // 🔥 community sync
            _context.CommunityPosts.Add(new CommunityPost
            {
                Content = post.Content,
                ImagePath = post.ImagePath,
                UserId = user.Id,
                CreatedAt = post.CreatedAt,
                OriginalPostType = "Student",
                OriginalPostId = post.Id
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Profile");
        }

        // ================= LIKE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var existing = await _context.StudentPostLikes
                .FirstOrDefaultAsync(x => x.StudentPostId == postId && x.UserId == user.Id);

            if (existing == null)
            {
                _context.StudentPostLikes.Add(new StudentPostLike
                {
                    StudentPostId = postId,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                _context.StudentPostLikes.Remove(existing);
            }

            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        // ================= COMMENT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CreateCommentViewModel model, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(model.Content))
            {
                _context.StudentPostComments.Add(new StudentPostComment
                {
                    StudentPostId = model.PostId,
                    UserId = user.Id,
                    Content = model.Content.Trim(),
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return SafeRedirect(returnUrl);
        }

        // ================= LIKE COMMENT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int commentId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var existing = await _context.StudentPostCommentLikes
                .FirstOrDefaultAsync(x => x.StudentPostCommentId == commentId && x.UserId == user.Id);

            if (existing == null)
            {
                _context.StudentPostCommentLikes.Add(new StudentPostCommentLike
                {
                    StudentPostCommentId = commentId,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                _context.StudentPostCommentLikes.Remove(existing);
            }

            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        // ================= ARCHIVE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int postId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _context.StudentPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post == null)
                return RedirectToAction("Index", "Profile");

            post.IsArchived = true;
            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        // ================= RESTORE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _context.StudentPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post != null)
            {
                post.IsArchived = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "StudentArchive");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _context.StudentPosts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post == null)
                return RedirectToAction("Index", "Profile");

            // 🔥 delete community
            var communityPosts = await _context.CommunityPosts
                .Where(p => p.OriginalPostType == "Student" && p.OriginalPostId == post.Id)
                .ToListAsync();

            foreach (var cp in communityPosts)
            {
                var comments = await _context.CommunityComments
                    .Where(c => c.CommunityPostId == cp.Id)
                    .ToListAsync();

                _context.CommunityComments.RemoveRange(comments);
            }

            _context.CommunityPosts.RemoveRange(communityPosts);

            // 🔥 delete student data
            var commentsStudent = await _context.StudentPostComments
                .Where(c => c.StudentPostId == post.Id)
                .ToListAsync();

            _context.StudentPostComments.RemoveRange(commentsStudent);
            _context.StudentPostLikes.RemoveRange(post.Likes ?? new List<StudentPostLike>());
            _context.StudentPosts.Remove(post);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Profile");
        }

        private IActionResult SafeRedirect(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Profile");
        }
    }
}