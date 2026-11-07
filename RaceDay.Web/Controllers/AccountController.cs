using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Controllers;

public class AccountController(IRaceDayApiClient api) : Controller {
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private IActionResult RedirectHome(UserSession user) => user.IsOrganiser ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Index", "Home");

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) {
        if (HttpContext.Session.GetUser() is { } user) return RedirectHome(user);
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model) {
        if (!ModelState.IsValid) return View(model);

        var result = await api.LoginAsync(new LoginPayload(model.Email.Trim(), model.Password));
        if (!result.Success) {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't log you in. Please try again.");
            model.Password = "";
            return View(model);
        }

        var r = result.Data!;
        HttpContext.Session.Clear();   // fresh session on login
        var user = new UserSession(r.Token, r.UserId, r.FirstName, r.LastName, r.Email, r.Role, r.ProfilePictureUrl);
        HttpContext.Session.SetUser(user);
        TempData["Success"] = $"Welcome back, {user.FirstName}!";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
        return RedirectHome(user);
    }

    [HttpGet]
    public IActionResult Register() {
        if (HttpContext.Session.GetUser() is { } user) return RedirectHome(user);
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model) {
        if (!ModelState.IsValid) return View(model);

        var result = await api.RegisterAsync(new RegisterPayload(model.FirstName.Trim(), model.LastName.Trim(), model.Email.Trim(), model.Password, model.Role));
        if (!result.Success) {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't create your account. Please try again.");
            model.Password = model.ConfirmPassword = "";
            return View(model);
        }

        TempData["Success"] = "Your account is ready. Log in to get started.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public IActionResult Logout() {
        HttpContext.Session.Clear();
        TempData["Success"] = "You've been logged out.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        ViewData["ErrorCode"] = "403";
        ViewData["ErrorTitle"] = "Access denied";
        ViewData["ErrorMessage"] = "That page belongs to a different type of account.";
        return View("~/Views/Shared/Error.cshtml");
    }

    [HttpGet, SessionAuthorize]
    public async Task<IActionResult> Profile() {
        var result = await api.GetProfileAsync();
        if (!result.Success) {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index", "Home");
        }
        var p = result.Data!;
        return View(new ProfileViewModel { FirstName = p.FirstName, LastName = p.LastName, Email = p.Email, Role = p.Role, ProfilePictureUrl = p.ProfilePictureUrl });
    }

    [HttpPost, SessionAuthorize]
    public async Task<IActionResult> Profile(ProfileViewModel model) {
        var user = HttpContext.Session.GetUser()!;
        model.Role = user.Role;
        model.ProfilePictureUrl = user.ProfilePictureUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await api.UpdateProfileAsync(new ProfilePayload(model.FirstName.Trim(), model.LastName.Trim(), model.Email.Trim()));
        if (!result.Success) {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't update your profile.");
            return View(model);
        }

        var p = result.Data!;
        HttpContext.Session.SetUser(user with { FirstName = p.FirstName, LastName = p.LastName, Email = p.Email, ProfilePictureUrl = p.ProfilePictureUrl ?? user.ProfilePictureUrl });
        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost, SessionAuthorize]
    public async Task<IActionResult> UploadPicture(IFormFile? picture) {
        if (picture is null || picture.Length == 0) { TempData["Error"] = "Choose an image to upload."; return RedirectToAction(nameof(Profile)); }
        if (picture.Length > MaxImageBytes || !ImageExtensions.Contains(Path.GetExtension(picture.FileName), StringComparer.OrdinalIgnoreCase)) {
            TempData["Error"] = "Upload a JPG, PNG, GIF or WebP image of 5 MB or less.";
            return RedirectToAction(nameof(Profile));
        }

        var result = await api.UploadProfilePictureAsync(picture);
        if (!result.Success) { TempData["Error"] = result.Error; return RedirectToAction(nameof(Profile)); }

        var user = HttpContext.Session.GetUser()!;
        HttpContext.Session.SetUser(user with { ProfilePictureUrl = result.Data!.ProfilePictureUrl });
        TempData["Success"] = "Your profile picture has been updated.";
        return RedirectToAction(nameof(Profile));
    }
}
