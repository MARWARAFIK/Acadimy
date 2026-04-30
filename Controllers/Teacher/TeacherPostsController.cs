using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.Models.Community;
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
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(content) && image == null)
                return RedirectToAction("Index", "TeacherProfile");

            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "teacher-posts");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                imagePath = "/uploads/teacher-posts/" + fileName;
            }

            var teacherPost = new TeacherPost
            {
                Content = content ?? string.Empty,
                ImagePath = imagePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };

            _context.TeacherPosts.Add(teacherPost);
            await _context.SaveChangesAsync();

            _context.CommunityPosts.Add(new CommunityPost
            {
                Content = teacherPost.Content,
                ImagePath = teacherPost.ImagePath,
                UserId = user.Id,
                CreatedAt = teacherPost.CreatedAt,
                OriginalPostType = "Teacher",
                OriginalPostId = teacherPost.Id
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "TeacherProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var existing = await _context.TeacherPostLikes
                .FirstOrDefaultAsync(x => x.TeacherPostId == postId && x.UserId == user.Id);

            if (existing == null)
            {
                _context.TeacherPostLikes.Add(new TeacherPostLike
                {
                    TeacherPostId = postId,
                    UserId = user.Id
                });
            }
            else
            {
                _context.TeacherPostLikes.Remove(existing);
            }

            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(content))
            {
                _context.TeacherPostComments.Add(new TeacherPostComment
                {
                    TeacherPostId = postId,
                    UserId = user.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now,
                    ParentCommentId = null
                });

                await _context.SaveChangesAsync();
            }

            return SafeRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyComment(int postId, int parentCommentId, string content, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var parentComment = await _context.TeacherPostComments
                .FirstOrDefaultAsync(c => c.Id == parentCommentId && c.TeacherPostId == postId);

            if (parentComment != null && !string.IsNullOrWhiteSpace(content))
            {
                _context.TeacherPostComments.Add(new TeacherPostComment
                {
                    TeacherPostId = postId,
                    ParentCommentId = parentCommentId,
                    UserId = user.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return SafeRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int commentId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var existing = await _context.TeacherPostCommentLikes
                .FirstOrDefaultAsync(x => x.TeacherPostCommentId == commentId && x.UserId == user.Id);

            if (existing == null)
            {
                _context.TeacherPostCommentLikes.Add(new TeacherPostCommentLike
                {
                    TeacherPostCommentId = commentId,
                    UserId = user.Id
                });
            }
            else
            {
                _context.TeacherPostCommentLikes.Remove(existing);
            }

            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int postId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var post = await _context.TeacherPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post == null)
                return RedirectToAction("Index", "TeacherProfile");

            post.IsArchived = true;
            await _context.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

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
            if (user == null) return RedirectToAction("Login", "Account");

            var post = await _context.TeacherPosts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post == null)
                return RedirectToAction("Index", "TeacherProfile");

            var communityPosts = await _context.CommunityPosts
                .Include(p => p.Likes)
                .Where(p => p.OriginalPostType == "Teacher" && p.OriginalPostId == post.Id)
                .ToListAsync();

            foreach (var cp in communityPosts)
            {
                var comments = await _context.CommunityComments
                    .Where(c => c.CommunityPostId == cp.Id)
                    .Include(c => c.Likes)
                    .ToListAsync();

                var commentLikes = comments.SelectMany(c => c.Likes).ToList();

                _context.CommunityCommentLikes.RemoveRange(commentLikes);
                _context.CommunityComments.RemoveRange(comments);
                _context.CommunityPostLikes.RemoveRange(cp.Likes);
            }

            _context.CommunityPosts.RemoveRange(communityPosts);

            var teacherComments = await _context.TeacherPostComments
                .Where(c => c.TeacherPostId == post.Id)
                .Include(c => c.Likes)
                .ToListAsync();

            var teacherCommentLikes = teacherComments.SelectMany(c => c.Likes).ToList();

            _context.TeacherPostCommentLikes.RemoveRange(teacherCommentLikes);
            _context.TeacherPostComments.RemoveRange(teacherComments);
            _context.TeacherPostLikes.RemoveRange(post.Likes);
            _context.TeacherPosts.Remove(post);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "TeacherProfile");
        }

        private IActionResult SafeRedirect(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "TeacherProfile");
        }
    }
}