using Microsoft.AspNetCore.SignalR;
using Notification.Core.Models;
using Notification.Infrastructure.Services;

namespace Notification.Api.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery
/// </summary>
public sealed class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            ConnectionTracker.AddConnection(userId, Context.ConnectionId);

            _logger.LogInformation(
                "User {UserId} connected with connection {ConnectionId}",
                userId,
                Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            ConnectionTracker.RemoveConnection(userId, Context.ConnectionId);

            _logger.LogInformation(
                "User {UserId} disconnected from connection {ConnectionId}",
                userId,
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client can call this to acknowledge receipt of a notification
    /// </summary>
    public async Task AcknowledgeNotification(Guid notificationId)
    {
        var userId = GetUserId();

        _logger.LogInformation(
            "User {UserId} acknowledged notification {NotificationId}",
            userId,
            notificationId);

        // Broadcast acknowledgment to other clients of the same user
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.OthersInGroup(userId).SendAsync("NotificationAcknowledged", notificationId);
        }
    }

    /// <summary>
    /// Client can call this to mark all notifications as read
    /// </summary>
    public async Task MarkAllAsRead()
    {
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} marked all notifications as read", userId);
            await Clients.OthersInGroup(userId).SendAsync("AllNotificationsRead");
        }
    }

    private string? GetUserId()
    {
        // Try to get user ID from claims first
        var userId = Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("userId")?.Value
            ?? Context.User?.Identity?.Name;

        // Fall back to query string parameter for anonymous connections
        if (string.IsNullOrEmpty(userId))
        {
            var httpContext = Context.GetHttpContext();
            userId = httpContext?.Request.Query["userId"].FirstOrDefault();
        }

        return userId;
    }
}
