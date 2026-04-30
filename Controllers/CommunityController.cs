using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Community;
using Acadimy.Models.Student;
using Acadimy.Models.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CommunityController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CommunityPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content, IFormFile? image)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(content) && image == null)
                return RedirectToAction(nameof(Index));

            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "community");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                imagePath = "/uploads/community/" + fileName;
            }

            _context.CommunityPosts.Add(new CommunityPost
            {
                Content = content ?? "",
                ImagePath = imagePath,
                UserId = currentUser.Id,
                CreatedAt = DateTime.Now,
                OriginalPostType = "Community",
                OriginalPostId = 0
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeAjax(int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Json(new { success = false, message = "Not authenticated" });

            var postExists = await _context.CommunityPosts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return Json(new { success = false, message = "Post not found" });

            var existingLike = await _context.CommunityPostLikes
                .FirstOrDefaultAsync(l => l.CommunityPostId == postId && l.UserId == currentUser.Id);

            bool liked;

            if (existingLike == null)
            {
                _context.CommunityPostLikes.Add(new CommunityPostLike
                {
                    CommunityPostId = postId,
                    UserId = currentUser.Id
                });

                liked = true;
            }
            else
            {
                _context.CommunityPostLikes.Remove(existingLike);
                liked = false;
            }

            await _context.SaveChangesAsync();

            var count = await _context.CommunityPostLikes
                .CountAsync(l => l.CommunityPostId == postId);

            return Json(new { success = true, liked, count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentAjax(int postId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Json(new { success = false, message = "Not authenticated" });

            if (string.IsNullOrWhiteSpace(content))
                return Json(new { success = false, message = "Comment is empty" });

            var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return Json(new { success = false, message = "Post not found" });

            var comment = new CommunityComment
            {
                CommunityPostId = postId,
                UserId = currentUser.Id,
                Content = content.Trim(),
                CreatedAt = DateTime.Now,
                ParentCommentId = null
            };

            _context.CommunityComments.Add(comment);

            if (post.OriginalPostType == "Teacher" && post.OriginalPostId > 0)
            {
                _context.TeacherPostComments.Add(new TeacherPostComment
                {
                    TeacherPostId = post.OriginalPostId,
                    UserId = currentUser.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now,
                    ParentCommentId = null
                });
            }
            else if (post.OriginalPostType == "Student" && post.OriginalPostId > 0)
            {
                _context.StudentPostComments.Add(new StudentPostComment
                {
                    StudentPostId = post.OriginalPostId,
                    UserId = currentUser.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            var fullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = currentUser.UserName ?? "Utilisateur";

            return Json(new
            {
                success = true,
                commentId = comment.Id,
                fullName,
                profileImage = string.IsNullOrWhiteSpace(currentUser.ProfileImagePath)
                    ? "/images/default-user.png"
                    : currentUser.ProfileImagePath,
                content = comment.Content,
                createdAt = comment.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyAjax(int commentId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Json(new { success = false, message = "Not authenticated" });

            if (string.IsNullOrWhiteSpace(content))
                return Json(new { success = false, message = "Reply is empty" });

            var parent = await _context.CommunityComments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (parent == null || parent.Post == null)
                return Json(new { success = false, message = "Comment not found" });

            var reply = new CommunityComment
            {
                CommunityPostId = parent.CommunityPostId,
                ParentCommentId = parent.Id,
                UserId = currentUser.Id,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.CommunityComments.Add(reply);

            if (parent.Post.OriginalPostType == "Teacher" && parent.Post.OriginalPostId > 0)
            {
                _context.TeacherPostComments.Add(new TeacherPostComment
                {
                    TeacherPostId = parent.Post.OriginalPostId,
                    UserId = currentUser.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now,
                    ParentCommentId = null
                });
            }
            else if (parent.Post.OriginalPostType == "Student" && parent.Post.OriginalPostId > 0)
            {
                _context.StudentPostComments.Add(new StudentPostComment
                {
                    StudentPostId = parent.Post.OriginalPostId,
                    UserId = currentUser.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            var fullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = currentUser.UserName ?? "Utilisateur";

            return Json(new
            {
                success = true,
                replyId = reply.Id,
                parentCommentId = parent.Id,
                fullName,
                profileImage = string.IsNullOrWhiteSpace(currentUser.ProfileImagePath)
                    ? "/images/default-user.png"
                    : currentUser.ProfileImagePath,
                content = reply.Content,
                createdAt = reply.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeCommentAjax(int commentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Json(new { success = false, message = "Not authenticated" });

            var commentExists = await _context.CommunityComments.AnyAsync(c => c.Id == commentId);
            if (!commentExists)
                return Json(new { success = false, message = "Comment not found" });

            var existingLike = await _context.CommunityCommentLikes
                .FirstOrDefaultAsync(l => l.CommunityCommentId == commentId && l.UserId == currentUser.Id);

            bool liked;

            if (existingLike == null)
            {
                _context.CommunityCommentLikes.Add(new CommunityCommentLike
                {
                    CommunityCommentId = commentId,
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.Now
                });

                liked = true;
            }
            else
            {
                _context.CommunityCommentLikes.Remove(existingLike);
                liked = false;
            }

            await _context.SaveChangesAsync();

            var count = await _context.CommunityCommentLikes
                .CountAsync(l => l.CommunityCommentId == commentId);

            return Json(new { success = true, liked, count });
        }
    }
}