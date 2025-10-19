using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

/// <summary>
/// Service interface for handling real-time notifications via SignalR
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send notification from admin to all users
    /// </summary>
    Task SendAdminNotificationAsync(string title, string message, string type = "info");

    /// <summary>
    /// Notify admins when a new waste report is created
    /// </summary>
    Task NotifyAdminsOfNewReportAsync(Report report);

    /// <summary>
    /// Notify user when their report status changes
    /// </summary>
    Task NotifyUserOfReportStatusChangeAsync(Guid userId, Report report);

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    Task SendNotificationToUserAsync(Guid userId, string title, string message, string type = "info");

    /// <summary>
    /// Send notification to specific group of users
    /// </summary>
    Task SendNotificationToGroupAsync(string groupName, string title, string message, string type = "info");
    Task SendReportCountToGroupAsync(int value, Guid focusId);
}
