using Notification.Core.Models;

namespace Notification.Core.Interfaces;

/// <summary>
/// Real-time notification service for pushing notifications to connected clients
/// </summary>
public interface IRealTimeNotificationService
{
    /// <summary>
    /// Sends a notification to a specific user
    /// </summary>
    Task SendToUserAsync(string userId, RealTimeNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to multiple users
    /// </summary>
    Task SendToUsersAsync(IEnumerable<string> userIds, RealTimeNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all connected clients
    /// </summary>
    Task BroadcastAsync(RealTimeNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of connected clients for a user
    /// </summary>
    Task<int> GetConnectionCountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is currently connected
    /// </summary>
    Task<bool> IsUserConnectedAsync(string userId, CancellationToken cancellationToken = default);
}
