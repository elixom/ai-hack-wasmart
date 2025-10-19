using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using GreenSync.Lib.Hubs;
using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

/// <summary>
/// SignalR-based notification service implementation
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Send notification from admin to all users
    /// </summary>
    public async Task SendAdminNotificationAsync(string title, string message, string type = "info")
    {
        _logger.LogInformation("Sending admin notification: {Title}", title);

        var notification = new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow,
            FromAdmin = true
        };

        await _hubContext.Clients.Group("RegularUsers").SendAsync("ReceiveAdminNotification", notification);
        _logger.LogInformation("Admin notification sent successfully");
    }

    /// <summary>
    /// Notify admins when a new waste report is created
    /// </summary>
    public async Task NotifyAdminsOfNewReportAsync(Report report)
    {
        _logger.LogInformation("Notifying admins of new report: {ReportId}", report.Id);

        var notification = new
        {
            Title = "New Waste Report",
            Message = $"A new {report.WasteType} waste report has been submitted at {report.Location}",
            Type = "info",
            Timestamp = DateTime.UtcNow,
            ReportId = report.Id,
            Priority = report.Priority.ToString(),
            Location = report.Location,
            WasteType = report.WasteType,
            Volume = report.EstimatedVolume
        };

        await _hubContext.Clients.Group("AdminUsers").SendAsync("ReceiveNewReportNotification", notification);
        _logger.LogInformation("New report notification sent to admins");
    }

    /// <summary>
    /// Notify user when their report status changes
    /// </summary>
    public async Task NotifyUserOfReportStatusChangeAsync(Guid userId, Report report)
    {
        _logger.LogInformation("Notifying user {UserId} of report status change: {ReportId} -> {Status}", userId, report.Id, report.Status);

        var statusMessage = report.Status switch
        {
            ReportStatus.Assigned => "Your waste report has been assigned to a collection team",
            ReportStatus.InProgress => "Collection team is on their way to your location",
            ReportStatus.Collected => "Your waste has been successfully collected. Thank you!",
            _ => $"Your report status has been updated to {report.Status}"
        };

        var notification = new
        {
            Title = "Report Status Update",
            Message = statusMessage,
            Type = report.Status == ReportStatus.Collected ? "success" : "info",
            Timestamp = DateTime.UtcNow,
            ReportId = report.Id,
            Status = report.Status.ToString(),
            Location = report.Location
        };

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveReportStatusUpdate", notification);
        _logger.LogInformation("Report status notification sent to user {UserId}", userId);
    }

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task SendNotificationToUserAsync(Guid userId, string title, string message, string type = "info")
    {
        _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, title);

        var notification = new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        _logger.LogInformation("Notification sent to user {UserId}", userId);
    }

    /// <summary>
    /// Send notification to specific group of users
    /// </summary>
    public async Task SendNotificationToGroupAsync(string groupName, string title, string message, string type = "info")
    {
        _logger.LogInformation("Sending notification to group {GroupName}: {Title}", groupName, title);

        var notification = new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        _logger.LogInformation("Notification sent to group {GroupName}", groupName);
    }
}
