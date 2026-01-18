namespace EventStore.Core.Interfaces;

/// <summary>
/// Unit of work for the Event Store.
/// </summary>
public interface IEventStoreUnitOfWork : IDisposable
{
    IEventRepository Events { get; }
    ISnapshotRepository Snapshots { get; }
    ISubscriptionRepository Subscriptions { get; }
    IDeadLetterRepository DeadLetters { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
