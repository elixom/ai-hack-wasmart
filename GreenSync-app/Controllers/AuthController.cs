using Microsoft.AspNetCore.Mvc;
using GreenSync.Lib.Services;

namespace GreenSync.App.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Username and password are required";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var result = await _authService.LoginAsync(username, password);

        if (result.Success)
        {
            // Redirect based on role
            if (result.User?.Role == UserRole.Admin)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                ? Redirect(returnUrl) 
                : RedirectToAction("Index", "Home");
        }

        ViewBag.Error = result.Message;
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
