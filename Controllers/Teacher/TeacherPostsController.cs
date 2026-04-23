using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public TeacherPostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content, IFormFile? image)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(content) && image == null)
                return RedirectToAction("Index", "TeacherProfile");

            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-posts");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await image.CopyToAsync(stream);

                imagePath = "/uploads/teacher-posts/" + fileName;
            }

            var post = new TeacherPost
            {
                Content = content?.Trim() ?? "",
                ImagePath = imagePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };

            _context.TeacherPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var existingLike = await _context.TeacherPostLikes
                .FirstOrDefaultAsync(l => l.TeacherPostId == postId && l.UserId == user.Id);

            if (existingLike == null)
            {
                _context.TeacherPostLikes.Add(new TeacherPostLike
                {
                    TeacherPostId = postId,
                    UserId = user.Id
                });
            }
            else
            {
                _context.TeacherPostLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(content))
            {
                var comment = new TeacherPostComment
                {
                    TeacherPostId = postId,
                    UserId = user.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now,
                    ParentCommentId = null
                };

                _context.TeacherPostComments.Add(comment);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyComment(int postId, int parentCommentId, string content, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(content))
            {
                var parentComment = await _context.TeacherPostComments
                    .FirstOrDefaultAsync(c => c.Id == parentCommentId && c.TeacherPostId == postId);

                if (parentComment != null)
                {
                    var reply = new TeacherPostComment
                    {
                        TeacherPostId = postId,
                        ParentCommentId = parentCommentId,
                        UserId = user.Id,
                        Content = content.Trim(),
                        CreatedAt = DateTime.Now
                    };

                    _context.TeacherPostComments.Add(reply);
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int commentId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var existingLike = await _context.TeacherPostCommentLikes
                .FirstOrDefaultAsync(l => l.TeacherPostCommentId == commentId && l.UserId == user.Id);

            if (existingLike == null)
            {
                _context.TeacherPostCommentLikes.Add(new TeacherPostCommentLike
                {
                    TeacherPostCommentId = commentId,
                    UserId = user.Id
                });
            }
            else
            {
                _context.TeacherPostCommentLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, string? returnUrl = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var comment = await _context.TeacherPostComments
                .Include(c => c.TeacherPost)
                .Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "TeacherProfile");
            }

            bool isMyComment = comment.UserId == currentUser.Id;
            bool isCommentOnMyPost = comment.TeacherPost != null && comment.TeacherPost.UserId == currentUser.Id;

            if (!isMyComment && !isCommentOnMyPost)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "TeacherProfile");
            }

            var replies = await _context.TeacherPostComments
                .Where(c => c.ParentCommentId == comment.Id)
                .Include(c => c.Likes)
                .ToListAsync();

            var replyLikes = replies.SelectMany(r => r.Likes).ToList();

            _context.TeacherPostCommentLikes.RemoveRange(replyLikes);
            _context.TeacherPostCommentLikes.RemoveRange(comment.Likes);
            _context.TeacherPostComments.RemoveRange(replies);
            _context.TeacherPostComments.Remove(comment);

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int postId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _context.TeacherPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post != null)
            {
                post.IsArchived = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "TeacherArchive");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int postId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _context.TeacherPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post != null)
            {
                post.IsArchived = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "TeacherArchive");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int postId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _context.TeacherPosts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post != null)
            {
                var allComments = await _context.TeacherPostComments
                    .Where(c => c.TeacherPostId == post.Id)
                    .Include(c => c.Likes)
                    .ToListAsync();

                var allCommentLikes = allComments.SelectMany(c => c.Likes).ToList();

                _context.TeacherPostCommentLikes.RemoveRange(allCommentLikes);
                _context.TeacherPostComments.RemoveRange(allComments);
                _context.TeacherPostLikes.RemoveRange(post.Likes);
                _context.TeacherPosts.Remove(post);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "TeacherProfile");
        }
    }
}