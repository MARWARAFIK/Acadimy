using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Live;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Controllers
{
    [Authorize]
    public class LiveClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LiveClassesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var classes = await _context.LiveClasses
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(classes);
        }

        [Authorize(Roles = "Enseignant")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Enseignant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LiveClass model)
        {
            var teacherId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            model.TeacherId = teacherId;
            model.IsActive = true;
            model.CreatedAt = DateTime.Now;

            _context.LiveClasses.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Room), new { id = model.Id });
        }

        public async Task<IActionResult> Room(int id)
        {
            var userId = _userManager.GetUserId(User);

            var live = await _context.LiveClasses
                .FirstOrDefaultAsync(x => x.Id == id);

            if (live == null)
                return NotFound();

            if (!live.IsActive)
                return RedirectToAction(nameof(Index));

            ViewBag.ClassId = live.Id;
            ViewBag.IsOwner = live.TeacherId == userId;

            return View(live);
        }

        [HttpPost]
        [Authorize(Roles = "Enseignant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> End(int id)
        {
            var userId = _userManager.GetUserId(User);

            var live = await _context.LiveClasses
                .FirstOrDefaultAsync(x => x.Id == id);

            if (live == null)
                return NotFound();

            if (live.TeacherId != userId)
                return Forbid();

            live.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}