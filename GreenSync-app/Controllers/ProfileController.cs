using Microsoft.AspNetCore.Mvc;
using GreenSync.Lib.Services;
using GreenSync_app.Models;

namespace GreenSync_app.Controllers;

public class ProfileController : Controller
{
    private readonly IAuthService _authService;
    private readonly IReportService _reportService;
    private readonly IEcoCreditService _ecoCreditService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IAuthService authService,
        IReportService reportService,
        IEcoCreditService ecoCreditService,
        ILogger<ProfileController> logger)
    {
        _authService = authService;
        _reportService = reportService;
        _ecoCreditService = ecoCreditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var userReports = await _reportService.GetReportsByUserIdAsync(currentUser.Id);
        var ecoCredit = await _ecoCreditService.GetEcoCreditByUserIdAsync(currentUser.Id);

        var viewModel = new ProfileViewModel
        {
            User = currentUser,
            TotalReports = userReports.Count(),
            EcoCreditsBalance = ecoCredit?.CurrentBalance ?? 0,
            MemberSince = currentUser.CreatedAt
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = new EditProfileViewModel
        {
            FullName = currentUser.FullName,
            Email = currentUser.Email
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Note: In a real implementation, you would call a service method to update the user
        // For now, we'll show a message that this is a demo
        _logger.LogInformation("User {UserId} attempted to update profile", currentUser.Id);
        
        TempData["Info"] = "Profile update is currently in demo mode. In production, this would save your changes to the database.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate passwords match
        if (model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "The new password and confirmation password do not match.");
            return View(model);
        }

        // Validate password strength
        if (model.NewPassword.Length < 6)
        {
            ModelState.AddModelError("NewPassword", "Password must be at least 6 characters long.");
            return View(model);
        }

        // Note: In a real implementation, you would:
        // 1. Verify the current password
        // 2. Hash and save the new password
        // For now, we'll show a message that this is a demo
        _logger.LogInformation("User {UserId} attempted to change password", currentUser.Id);
        
        TempData["Info"] = "Password change is currently in demo mode. In production, this would update your password securely.";
        return RedirectToAction("Index");
    }
}
