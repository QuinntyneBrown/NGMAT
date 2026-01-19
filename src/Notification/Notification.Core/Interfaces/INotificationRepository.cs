using Notification.Core.Models;

namespace Notification.Core.Interfaces;

public interface INotificationRepository
{
    Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserNotification>> GetByUserAsync(string userId, NotificationFilter? filter = null, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}

public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task SaveAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default);
}
