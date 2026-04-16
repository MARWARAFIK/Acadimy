using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Acadimy.Models;
using Acadimy.ViewModels;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ================= LOGIN =================

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
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

            // 🔥 REDIRECTION SELON ROLE
            if (await _userManager.IsInRoleAsync(user, "Student"))
                return RedirectToAction("IndexStudent", "Home");

            else if (await _userManager.IsInRoleAsync(user, "Teacher"))
                return RedirectToAction("IndexTeacher", "Home");

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Message = "Email ou mot de passe incorrect";
        return View(model);
    }

    // ================= REGISTER =================

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
            await _signInManager.SignInAsync(user, false);

            if (model.Role == "Student")
                return RedirectToAction("IndexStudent", "Home");

            else
                return RedirectToAction("IndexTeacher", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    // ================= RESET PASSWORD DIRECT =================

    [HttpPost]
    public async Task<IActionResult> ResetPasswordDirect(string email, string newPassword)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
        {
            ViewBag.Message = "Remplir tous les champs";
            return View("Login");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            ViewBag.Message = "Utilisateur non trouvé";
            return View("Login");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            ViewBag.Message = "Mot de passe modifié avec succès";
            return View("Login");
        }

        ViewBag.Message = "Erreur modification mot de passe";
        return View("Login");
    }

    // ================= LOGOUT =================

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}