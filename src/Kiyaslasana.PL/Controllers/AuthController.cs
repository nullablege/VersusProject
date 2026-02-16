using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kiyaslasana.PL.Controllers;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AuthController : SeoControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("/giris")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        SetSeo(
            title: "Giris",
            description: "Kiyaslasana hesabinizla giris yapin.",
            canonicalUrl: BuildAbsoluteUrl("/giris"));
        ViewData["Robots"] = "noindex,follow";

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("/giris")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, [FromForm] string? returnUrl = null)
    {
        model.ReturnUrl = returnUrl ?? model.ReturnUrl;

        if (!ModelState.IsValid)
        {
            SetLoginSeo();
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Redirect(GetSafeReturnUrl(model.ReturnUrl));
        }

        ModelState.AddModelError(string.Empty, "Giris bilgileri gecersiz.");
        SetLoginSeo();
        return View(model);
    }

    [HttpGet("/kayit-ol")]
    public IActionResult Register([FromQuery] string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        SetSeo(
            title: "Kayit Ol",
            description: "Kiyaslasana uyesi olun.",
            canonicalUrl: BuildAbsoluteUrl("/kayit-ol"));
        ViewData["Robots"] = "noindex,follow";

        return View(new RegisterViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("/kayit-ol")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, [FromForm] string? returnUrl = null)
    {
        model.ReturnUrl = returnUrl ?? model.ReturnUrl;

        if (!ModelState.IsValid)
        {
            SetRegisterSeo();
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi zaten kayitli.");
            SetRegisterSeo();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            SetRegisterSeo();
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, IdentityRoles.Member);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            SetRegisterSeo();
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return Redirect(GetSafeReturnUrl(model.ReturnUrl));
    }

    [Authorize]
    [HttpPost("/cikis")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return Redirect(GetSafeReturnUrl(returnUrl));
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/";
    }

    private void SetLoginSeo()
    {
        SetSeo(
            title: "Giris",
            description: "Kiyaslasana hesabinizla giris yapin.",
            canonicalUrl: BuildAbsoluteUrl("/giris"));
        ViewData["Robots"] = "noindex,follow";
    }

    private void SetRegisterSeo()
    {
        SetSeo(
            title: "Kayit Ol",
            description: "Kiyaslasana uyesi olun.",
            canonicalUrl: BuildAbsoluteUrl("/kayit-ol"));
        ViewData["Robots"] = "noindex,follow";
    }
}
