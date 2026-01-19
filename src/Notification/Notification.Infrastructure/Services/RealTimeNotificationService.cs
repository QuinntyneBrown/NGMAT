using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Infrastructure.Services;

internal sealed class RealTimeNotificationService<THub> : IRealTimeNotificationService
    where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;
    private readonly ILogger<RealTimeNotificationService<THub>> _logger;

    public RealTimeNotificationService(
        IHubContext<THub> hubContext,
        ILogger<RealTimeNotificationService<THub>> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(
        string userId,
        RealTimeNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group(userId)
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogDebug(
                "Sent real-time notification {NotificationId} to user {UserId}",
                notification.Id,
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send real-time notification to user {UserId}",
                userId);
        }
    }

    public async Task SendToUsersAsync(
        IEnumerable<string> userIds,
        RealTimeNotification notification,
        CancellationToken cancellationToken = default)
    {
        var tasks = userIds.Select(userId => SendToUserAsync(userId, notification, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task BroadcastAsync(
        RealTimeNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogDebug("Broadcast notification {NotificationId} to all users", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast notification");
        }
    }

    public Task<int> GetConnectionCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ConnectionTracker.GetUserConnectionCount(userId));
    }

    public Task<bool> IsUserConnectedAsync(string userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ConnectionTracker.IsUserConnected(userId));
    }
}

/// <summary>
/// Tracks SignalR connections for users
/// </summary>
public static class ConnectionTracker
{
    private static readonly Dictionary<string, HashSet<string>> UserConnections = new();
    private static readonly object ConnectionLock = new();

    public static int GetUserConnectionCount(string userId)
    {
        lock (ConnectionLock)
        {
            return UserConnections.TryGetValue(userId, out var connections)
                ? connections.Count
                : 0;
        }
    }

    public static bool IsUserConnected(string userId)
    {
        lock (ConnectionLock)
        {
            return UserConnections.ContainsKey(userId);
        }
    }

    public static void AddConnection(string userId, string connectionId)
    {
        lock (ConnectionLock)
        {
            if (!UserConnections.TryGetValue(userId, out var connections))
            {
                connections = new HashSet<string>();
                UserConnections[userId] = connections;
            }
            connections.Add(connectionId);
        }
    }

    public static void RemoveConnection(string userId, string connectionId)
    {
        lock (ConnectionLock)
        {
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    UserConnections.Remove(userId);
                }
            }
        }
    }
}
