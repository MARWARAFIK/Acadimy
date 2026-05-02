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

        public async Task<IActionResult> Index(string? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var profileUser = string.IsNullOrWhiteSpace(id)
                ? await _context.Users
                    .Include(u => u.StudentSkills)
                    .FirstOrDefaultAsync(u => u.Id == currentUser.Id)
                : await _context.Users
                    .Include(u => u.StudentSkills)
                    .FirstOrDefaultAsync(u => u.Id == id);

            if (profileUser == null)
                return NotFound();

            var posts = await _context.StudentPosts
                .Where(p => p.UserId == profileUser.Id && !p.IsArchived)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostItemViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Content = p.Content,
                    ImagePath = p.ImagePath,
                    CreatedAt = p.CreatedAt,

                    FullName = ((profileUser.FirstName ?? "") + " " + (profileUser.LastName ?? "")).Trim(),
                    ProfileImagePath = profileUser.ProfileImagePath,
                    Filiere = profileUser.Filiere,
                    Niveau = profileUser.Niveau,

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
                            ProfileImagePath = c.User.ProfileImagePath,
                            LikesCount = _context.StudentPostCommentLikes
                                .Count(l => l.StudentPostCommentId == c.Id)
                        })
                        .ToList()
                })
                .ToListAsync();

            var model = new ProfileViewModel
            {
                FullName = $"{profileUser.FirstName} {profileUser.LastName}".Trim(),
                Email = profileUser.Email,
                Phone = profileUser.PhoneNumberCustom,
                Location = profileUser.Location,
                Website = profileUser.Website,
                Bio = profileUser.Bio,
                Filiere = profileUser.Filiere,
                Niveau = profileUser.Niveau,
                ProfileImagePath = profileUser.ProfileImagePath,
                CoverImagePath = profileUser.CoverImagePath,

                Skills = profileUser.StudentSkills
                    .Select(s => new StudentSkillItemViewModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Percentage = s.Percentage
                    })
                    .ToList(),

                Posts = posts
            };

            ViewBag.IsOwnProfile = profileUser.Id == currentUser.Id;
            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.ProfileUserId = profileUser.Id;
           
            return View("~/Views/Student/Profile/Index.cshtml", model);
        }
    }
}