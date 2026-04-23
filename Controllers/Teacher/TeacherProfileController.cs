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
    public class TeacherProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TeacherProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var user = await _userManager.Users
                .Include(u => u.TeacherExpertises)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var posts = await LoadPosts(user.Id);
            var model = BuildTeacherProfileViewModel(user, posts, currentUserId);

            ViewBag.IsOwnProfile = true;
            ViewBag.CurrentUserId = currentUserId;

            return View("~/Views/Teacher/Profile/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> ViewProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Index));

            var currentUserId = _userManager.GetUserId(User);

            var user = await _userManager.Users
                .Include(u => u.TeacherExpertises)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var posts = await LoadPosts(user.Id);
            var model = BuildTeacherProfileViewModel(user, posts, currentUserId);

            ViewBag.IsOwnProfile = user.Id == currentUserId;
            ViewBag.CurrentUserId = currentUserId;

            return View("~/Views/Teacher/Profile/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult TeacherProfile()
        {
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<TeacherPost>> LoadPosts(string userId)
        {
            var posts = await _context.TeacherPosts
                .Where(p => p.UserId == userId && !p.IsArchived)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postIds = posts.Select(p => p.Id).ToList();

            var allComments = await _context.TeacherPostComments
                .Where(c => postIds.Contains(c.TeacherPostId))
                .Include(c => c.User)
                .Include(c => c.Likes)
                .ToListAsync();

            foreach (var post in posts)
            {
                post.Comments = allComments
                    .Where(c => c.TeacherPostId == post.Id)
                    .ToList();
            }

            return posts;
        }

        private TeacherProfileViewModel BuildTeacherProfileViewModel(
            ApplicationUser user,
            List<TeacherPost> posts,
            string? currentUserId)
        {
            return new TeacherProfileViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? "",
                Bio = string.IsNullOrWhiteSpace(user.Bio)
                                    ? "Aucune biographie disponible." : user.Bio,
                Specialite = string.IsNullOrWhiteSpace(user.Specialite)
                                    ? "Non spécifiée" : user.Specialite,
                Grade = string.IsNullOrWhiteSpace(user.Grade)
                                    ? "Non spécifié" : user.Grade,
                Experience = user.Experience,
                ProfileImagePath = string.IsNullOrWhiteSpace(user.ProfileImagePath)
                                    ? "/images/default-user.png" : user.ProfileImagePath,
                CoverImagePath = string.IsNullOrWhiteSpace(user.CoverImagePath)
                                    ? "/images/teacher-banner-default.jpg" : user.CoverImagePath,
                Expertises = user.TeacherExpertises
                    .Select(e => new TeacherExpertiseItemViewModel
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Percentage = e.Percentage
                    })
                    .ToList(),

                Posts = posts.Select(p => new TeacherPostViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Content = p.Content,
                    ImagePath = p.ImagePath,
                    CreatedAt = p.CreatedAt,
                    IsArchived = p.IsArchived,
                    FullName = p.User == null
                                        ? "Unknown"
                                        : $"{p.User.FirstName} {p.User.LastName}".Trim(),
                    ProfileImagePath = p.User?.ProfileImagePath
                                        ?? "/images/default-user.png",
                    LikesCount = p.Likes?.Count ?? 0,
                    IsLikedByCurrentUser = p.Likes != null && currentUserId != null &&
                                          p.Likes.Any(l => l.UserId == currentUserId),

                    Comments = (p.Comments ?? new List<TeacherPostComment>())
                        .Where(c => c.ParentCommentId == null)
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new TeacherPostCommentViewModel
                        {
                            Id = c.Id,
                            UserId = c.UserId,
                            FullName = c.User == null
                                                ? "Unknown"
                                                : $"{c.User.FirstName} {c.User.LastName}".Trim(),
                            ProfileImagePath = c.User?.ProfileImagePath
                                                ?? "/images/default-user.png",
                            Content = c.Content,
                            CreatedAt = c.CreatedAt,
                            LikesCount = c.Likes?.Count ?? 0,
                            IsLikedByCurrentUser = c.Likes != null && currentUserId != null &&
                                                  c.Likes.Any(l => l.UserId == currentUserId),

                            Replies = (p.Comments ?? new List<TeacherPostComment>())
                                .Where(r => r.ParentCommentId == c.Id)
                                .OrderBy(r => r.CreatedAt)
                                .Select(r => new TeacherPostCommentViewModel
                                {
                                    Id = r.Id,
                                    UserId = r.UserId,
                                    FullName = r.User == null
                                                        ? "Unknown"
                                                        : $"{r.User.FirstName} {r.User.LastName}".Trim(),
                                    ProfileImagePath = r.User?.ProfileImagePath
                                                        ?? "/images/default-user.png",
                                    Content = r.Content,
                                    CreatedAt = r.CreatedAt,
                                    LikesCount = r.Likes?.Count ?? 0,
                                    IsLikedByCurrentUser = r.Likes != null && currentUserId != null &&
                                                          r.Likes.Any(l => l.UserId == currentUserId)
                                })
                                .ToList()
                        })
                        .ToList()
                }).ToList()
            };
        }
    }
}