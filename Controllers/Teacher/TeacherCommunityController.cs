using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Acadimy.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Teacher
{
    [Authorize(Roles = "Enseignant")]
    public class TeacherCommunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherCommunityController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var postsQuery = _context.TeacherPosts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.User)
            .Include(p => p.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Likes)
            .Include(p => p.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User)
            .Include(p => p.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.Likes)
            .Where(p => !p.IsArchived && p.UserId != currentUser.Id);

            List<TeacherPost> posts;

            if (string.IsNullOrWhiteSpace(currentUser.Specialite))
            {
                posts = await postsQuery
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                posts = await postsQuery
                    .Where(p => p.User != null && p.User.Specialite == currentUser.Specialite)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                if (!posts.Any())
                {
                    posts = await postsQuery
                        .OrderByDescending(p => p.CreatedAt)
                        .ToListAsync();
                }
            }

            var model = posts.Select(p => new TeacherPostViewModel
            {
                Id = p.Id,
                UserId = p.UserId,
                Content = p.Content,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                FullName = p.User == null
                    ? "Unknown User"
                    : $"{p.User.FirstName} {p.User.LastName}".Trim(),
                ProfileImagePath = p.User == null || string.IsNullOrWhiteSpace(p.User.ProfileImagePath)
                    ? "/images/default-user.png"
                    : p.User.ProfileImagePath,
                LikesCount = p.Likes?.Count ?? 0,
                IsLikedByCurrentUser = p.Likes != null &&
                                       p.Likes.Any(l => l.UserId == currentUser.Id),
                Comments = (p.Comments ?? new List<TeacherPostComment>())
                    .Where(c => c.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new TeacherPostCommentViewModel
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        FullName = c.User == null
                            ? "Unknown User"
                            : $"{c.User.FirstName} {c.User.LastName}".Trim(),
                        ProfileImagePath = c.User == null || string.IsNullOrWhiteSpace(c.User.ProfileImagePath)
                            ? "/images/default-user.png"
                            : c.User.ProfileImagePath,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        LikesCount = c.Likes?.Count ?? 0,
                        IsLikedByCurrentUser = c.Likes != null &&
                                               c.Likes.Any(l => l.UserId == currentUser.Id),
                        Replies = (c.Replies ?? new List<TeacherPostComment>())
                            .OrderBy(r => r.CreatedAt)
                            .Select(r => new TeacherPostCommentViewModel
                            {
                                Id = r.Id,
                                UserId = r.UserId,
                                FullName = r.User == null
                                    ? "Unknown User"
                                    : $"{r.User.FirstName} {r.User.LastName}".Trim(),
                                ProfileImagePath = r.User == null || string.IsNullOrWhiteSpace(r.User.ProfileImagePath)
                                    ? "/images/default-user.png"
                                    : r.User.ProfileImagePath,
                                Content = r.Content,
                                CreatedAt = r.CreatedAt,
                                LikesCount = r.Likes?.Count ?? 0,
                                IsLikedByCurrentUser = r.Likes != null &&
                                                       r.Likes.Any(l => l.UserId == currentUser.Id)
                            })
                            .ToList()
                    })
                    .ToList()
            }).ToList();

            ViewBag.CurrentUserId = currentUser.Id;

            return View("~/Views/Teacher/Community/Index.cshtml", model);
        }
    }
}