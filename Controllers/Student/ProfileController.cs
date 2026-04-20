using Acadimy.Data;
using Acadimy.Models;
using Acadimy.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers.Student
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var posts = await _context.StudentPosts
                .Where(p => p.UserId == user.Id)
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
                    FullName = (user.FirstName ?? "") + " " + (user.LastName ?? ""),
                    ProfileImagePath = user.ProfileImagePath,
                    Filiere = user.Filiere,
                    Niveau = user.Niveau,
                    LikesCount = p.Likes.Count,
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == user.Id),
                    Comments = p.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new PostCommentItemViewModel
                        {
                            Id = c.Id,
                            Content = c.Content,
                            CreatedAt = c.CreatedAt,
                            FullName = (c.User!.FirstName ?? "") + " " + (c.User!.LastName ?? ""),
                            ProfileImagePath = c.User.ProfileImagePath
                        })
                        .ToList()
                })
                .ToListAsync();

            var model = new ProfileViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Phone = user.PhoneNumberCustom,
                Location = user.Location,
                Website = user.Website,
                Bio = user.Bio,
                Filiere = user.Filiere,
                Niveau = user.Niveau,
                ProfileImagePath = user.ProfileImagePath,
                CoverImagePath = user.CoverImagePath,
                Skill = user.Skill,
                SkillPercent = user.SkillPercent,
                Posts = posts
            };

            return View("~/Views/Student/Profile/Index.cshtml", model);
        }
    }
}