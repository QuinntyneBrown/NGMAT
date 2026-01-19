using Notification.Core.Entities;
using Notification.Core.Models;

namespace Notification.Core.Interfaces;

public interface INotificationRepository
{
    Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(string userId, NotificationFilter? filter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationEntity>> GetPendingAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationEntity>> GetFailedForRetryAsync(int maxRetries = 3, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}

public interface INotificationPreferencesRepository
{
    Task<NotificationPreferencesEntity?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationPreferencesEntity preferences, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationPreferencesEntity preferences, CancellationToken cancellationToken = default);
}
