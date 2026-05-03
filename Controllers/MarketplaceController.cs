using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers
{
    [Authorize]
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public MarketplaceController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Marketplace
        public async Task<IActionResult> Index()
        {
            var projects = await _context.ProjectPosts
                .Include(p => p.User)
                .Include(p => p.Ratings)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        // GET: /Marketplace/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.ProjectPosts
                .Include(p => p.User)
                .Include(p => p.Ratings)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: /Marketplace/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title,
            string description,
            IFormFile? image,
            IFormFile? file)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                return RedirectToAction(nameof(Index));

            string? imagePath = null;
            string? filePath = null;

            var folder = Path.Combine(_env.WebRootPath, "uploads", "projects");
            Directory.CreateDirectory(folder);

            if (image != null && image.Length > 0)
            {
                var imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var imageFullPath = Path.Combine(folder, imageName);

                await using var stream = new FileStream(imageFullPath, FileMode.Create);
                await image.CopyToAsync(stream);

                imagePath = "/uploads/projects/" + imageName;
            }

            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var fileFullPath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(fileFullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                filePath = "/uploads/projects/" + fileName;
            }

            var project = new ProjectPost
            {
                Title = title.Trim(),
                Description = description.Trim(),
                ImagePath = imagePath,
                FilePath = filePath,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.ProjectPosts.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Marketplace/Rate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int projectId, int value)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var existing = await _context.ProjectRatings
                .FirstOrDefaultAsync(r => r.ProjectPostId == projectId && r.UserId == user.Id);

            if (existing == null)
            {
                _context.ProjectRatings.Add(new ProjectRating
                {
                    ProjectPostId = projectId,
                    UserId = user.Id,
                    Value = value
                });
            }
            else
            {
                existing.Value = value;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = projectId });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var project = await _context.ProjectPosts.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (project.UserId != user.Id)
                return Forbid();

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string title,
            string description,
            IFormFile? image,
            IFormFile? file)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var project = await _context.ProjectPosts.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (project.UserId != user.Id)
                return Forbid();

            project.Title = title.Trim();
            project.Description = description.Trim();

            var folder = Path.Combine(_env.WebRootPath, "uploads", "projects");
            Directory.CreateDirectory(folder);

            if (image != null && image.Length > 0)
            {
                var imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var imageFullPath = Path.Combine(folder, imageName);

                await using var stream = new FileStream(imageFullPath, FileMode.Create);
                await image.CopyToAsync(stream);

                project.ImagePath = "/uploads/projects/" + imageName;
            }

            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var fileFullPath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(fileFullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                project.FilePath = "/uploads/projects/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = project.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var project = await _context.ProjectPosts
                .Include(p => p.Ratings)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (project.UserId != user.Id)
                return Forbid();

            _context.ProjectRatings.RemoveRange(project.Ratings);
            _context.ProjectComments.RemoveRange(project.Comments);
            _context.ProjectPosts.Remove(project);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        // POST: /Marketplace/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int projectId, string content, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(content))
            {
                _context.ProjectComments.Add(new ProjectComment
                {
                    ProjectPostId = projectId,
                    UserId = user.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return SafeRedirect(returnUrl);
        }

        // GET: /Marketplace/Top
        public async Task<IActionResult> Top()
        {
            var projects = await _context.ProjectPosts
                .Include(p => p.User)
                .Include(p => p.Ratings)
                .Include(p => p.Comments)
                .ToListAsync();

            projects = projects
                .OrderByDescending(p => p.Ratings.Any() ? p.Ratings.Average(r => r.Value) : 0)
                .ThenByDescending(p => p.Ratings.Count)
                .ThenByDescending(p => p.Comments.Count)
                .ThenByDescending(p => p.CreatedAt)
                .ToList();

            return View(projects);
        }

        private IActionResult SafeRedirect(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}