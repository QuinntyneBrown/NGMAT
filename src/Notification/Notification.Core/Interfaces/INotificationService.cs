using Notification.Core.Entities;
using Notification.Core.Models;

namespace Notification.Core.Interfaces;

/// <summary>
/// Main notification orchestration service interface
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an email notification
    /// </summary>
    Task<NotificationSendResult> SendEmailAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a webhook notification
    /// </summary>
    Task<NotificationSendResult> SendWebhookAsync(WebhookNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an in-app notification
    /// </summary>
    Task<NotificationSendResult> SendInAppAsync(UserNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for a user
    /// </summary>
    Task<IReadOnlyList<NotificationEntity>> GetNotificationsAsync(string userId, NotificationFilter? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notification by ID
    /// </summary>
    Task<NotificationEntity?> GetNotificationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks notification as read
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications for a user as read
    /// </summary>
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread notification count for a user
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification
    /// </summary>
    Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries failed notifications
    /// </summary>
    Task RetryFailedNotificationsAsync(CancellationToken cancellationToken = default);
}
