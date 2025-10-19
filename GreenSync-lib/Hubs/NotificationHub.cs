using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using GreenSync.Lib.Models;

namespace GreenSync.Lib.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications between admin and users
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);

        // Add user to appropriate groups based on their role
        if (Context.User?.IsInRole("Administrator") == true || Context.User?.IsInRole("Supervisor") == true)
        {
            await Groups.AddToGroupAsync(connectionId, "AdminUsers");
            _logger.LogInformation("Added admin user {UserId} to AdminUsers group", userId);
        }
        else
        {
            await Groups.AddToGroupAsync(connectionId, "RegularUsers");
            _logger.LogInformation("Added regular user {UserId} to RegularUsers group", userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}", userId, connectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with error", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send notification from admin to all users
    /// </summary>
    [Authorize(Roles = "Administrator,Supervisor")]
    public async Task SendAdminNotification(string title, string message, string type = "info")
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("Admin {UserId} sending notification: {Title}", userId, title);

        var notification = new
        {
            Title = title,
            Message = message,
            Type = type, // info, warning, success, danger
            Timestamp = DateTime.UtcNow,
            FromAdmin = true
        };

        await Clients.Group("RegularUsers").SendAsync("ReceiveAdminNotification", notification);
    }

    /// <summary>
    /// Join a specific user group (for targeted notifications)
    /// </summary>
    public async Task JoinUserGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined group {GroupName}", Context.UserIdentifier, groupName);
    }

    /// <summary>
    /// Leave a specific user group
    /// </summary>
    public async Task LeaveUserGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} left group {GroupName}", Context.UserIdentifier, groupName);
    }
}
