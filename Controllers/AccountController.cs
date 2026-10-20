using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using organizador_dietas_santo_tomas.Data;
using organizador_dietas_santo_tomas.Models;
using organizador_dietas_santo_tomas.Services;

namespace organizador_dietas_santo_tomas.Controllers;

public class AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users,
    UserCreationService creator) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user is not null)
        {
            var result = await signIn.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente. Inténtelo más tarde.");
        }
        if (ModelState.ErrorCount == 0)
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role is not (AppRoles.Doctor or AppRoles.Patient))
            ModelState.AddModelError(nameof(model.Role), "Seleccione Médico o Paciente.");
        if (!ModelState.IsValid) return View(model);

        var result = await creator.CreateAsync(model.FullName, model.Email, model.Password, model.Role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        var user = (await users.FindByEmailAsync(model.Email.Trim()))!;
        await signIn.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
