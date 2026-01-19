using Notification.Core.Entities;
using Notification.Core.Models;

namespace Notification.Core.Interfaces;

/// <summary>
/// Repository for notification templates
/// </summary>
public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationTemplate>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
