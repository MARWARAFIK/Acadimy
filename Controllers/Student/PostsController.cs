using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Student;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Content) &&
                (model.Image == null || model.Image.Length == 0))
            {
                return RedirectToAction("Index", "Profile");
            }

            string? imagePath = null;

            if (model.Image != null && model.Image.Length > 0)
            {
                string folder = Path.Combine(_environment.WebRootPath, "uploads", "posts");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);

                imagePath = "/uploads/posts/" + fileName;
            }

            var post = new StudentPost
            {
                Content = model.Content ?? "",
                ImagePath = imagePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.StudentPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId, string returnTo = "Profile")
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var existingLike = await _context.StudentPostLikes
                .FirstOrDefaultAsync(x => x.StudentPostId == postId && x.UserId == user.Id);

            if (existingLike == null)
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
                _context.StudentPostLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            if (returnTo == "Explore")
                return RedirectToAction("Explore");

            return RedirectToAction("Index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CreateCommentViewModel model, string returnTo = "Profile")
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                if (returnTo == "Explore")
                    return RedirectToAction("Explore");

                return RedirectToAction("Index", "Profile");
            }

            var comment = new StudentPostComment
            {
                StudentPostId = model.PostId,
                UserId = user.Id,
                Content = model.Content,
                CreatedAt = DateTime.Now
            };

            _context.StudentPostComments.Add(comment);
            await _context.SaveChangesAsync();

            if (returnTo == "Explore")
                return RedirectToAction("Explore");

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Explore()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var posts = await _context.StudentPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostItemViewModel
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImagePath = p.ImagePath,
                    CreatedAt = p.CreatedAt,
                    FullName = ((p.User!.FirstName ?? "") + " " + (p.User!.LastName ?? "")).Trim(),
                    ProfileImagePath = p.User.ProfileImagePath,
                    Filiere = p.User.Filiere,
                    Niveau = p.User.Niveau,
                    LikesCount = p.Likes.Count,
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == currentUser.Id),
                    Comments = p.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new PostCommentItemViewModel
                        {
                            Id = c.Id,
                            Content = c.Content,
                            CreatedAt = c.CreatedAt,
                            FullName = ((c.User!.FirstName ?? "") + " " + (c.User!.LastName ?? "")).Trim(),
                            ProfileImagePath = c.User.ProfileImagePath
                        })
                        .ToList()
                })
                .ToListAsync();

            return View("~/Views/Student/Posts/Explore.cshtml", posts);
        }
    }
}