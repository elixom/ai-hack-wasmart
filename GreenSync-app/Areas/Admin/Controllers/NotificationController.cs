using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GreenSync.Lib.Services;
using Microsoft.Extensions.Logging;

namespace GreenSync_app.Areas.Admin.Controllers;

/// <summary>
/// Controller for admin notification management
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Administrator,Supervisor")]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// GET: Admin/Notification
    /// </summary>
    public IActionResult Index()
    {
        return View("Send");
    }

    /// <summary>
    /// GET: Admin/Notification/Send
    /// </summary>
    public IActionResult Send()
    {
        return View();
    }

    /// <summary>
    /// POST: Admin/Notification/Send
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string title, string message, string type = "info")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Title and message are required.";
                return View();
            }

            await _notificationService.SendAdminNotificationAsync(title, message, type);
            
            _logger.LogInformation("Admin notification sent: {Title} by {User}", title, User.Identity?.Name);
            TempData["SuccessMessage"] = "Notification sent successfully to all users.";
            
            return RedirectToAction(nameof(Send));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin notification");
            TempData["ErrorMessage"] = "Failed to send notification. Please try again.";
            return View();
        }
    }

    /// <summary>
    /// POST: Admin/Notification/SendToGroup
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToGroup(string groupName, string title, string message, string type = "info")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "Group name, title and message are required." });
            }

            await _notificationService.SendNotificationToGroupAsync(groupName, title, message, type);
            
            _logger.LogInformation("Group notification sent: {Title} to {Group} by {User}", title, groupName, User.Identity?.Name);
            
            return Json(new { success = true, message = $"Notification sent successfully to group '{groupName}'." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending group notification to {GroupName}", groupName);
            return Json(new { success = false, message = "Failed to send notification. Please try again." });
        }
    }
}
