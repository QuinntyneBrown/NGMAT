namespace Notification.Core.Interfaces;

/// <summary>
/// Unit of work pattern for coordinating repository operations
/// </summary>
public interface IUnitOfWork : IDisposable
{
    INotificationRepository Notifications { get; }
    INotificationPreferencesRepository Preferences { get; }
    INotificationTemplateRepository Templates { get; }
    IAlertRuleRepository AlertRules { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
