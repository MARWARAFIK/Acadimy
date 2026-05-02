using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Acadimy.Models;
using Acadimy.ViewModels;

namespace Acadimy.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                false
            );

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.IsBlocked)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError("", "Votre compte est bloqué par l'administrateur.");
                    return View(model);
                }

                if (user == null)
                {
                    ModelState.AddModelError("", "Utilisateur introuvable.");
                    return View(model);
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Admin");

                if (roles.Contains("Enseignant"))
                    return RedirectToAction("Index", "Teacher");

                if (roles.Contains("Étudiant"))
                    return RedirectToAction("Index", "Student");
            }

            ModelState.AddModelError("", "Email ou mot de passe incorrect.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (model.Role == "Enseignant")
                    return RedirectToAction("Index", "Teacher");

                return RedirectToAction("Index", "Student");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}