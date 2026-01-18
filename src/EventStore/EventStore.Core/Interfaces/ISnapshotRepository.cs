using EventStore.Core.Entities;

namespace EventStore.Core.Interfaces;

/// <summary>
/// Repository interface for snapshots.
/// </summary>
public interface ISnapshotRepository
{
    /// <summary>
    /// Saves a snapshot.
    /// </summary>
    Task<Snapshot> SaveAsync(Snapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest snapshot for an aggregate.
    /// </summary>
    Task<Snapshot?> GetLatestAsync(Guid aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all snapshots for an aggregate.
    /// </summary>
    Task<IReadOnlyList<Snapshot>> GetAllAsync(Guid aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes snapshots older than the specified date.
    /// </summary>
    Task<int> DeleteOlderThanAsync(Guid aggregateId, DateTimeOffset date, CancellationToken cancellationToken = default);
}
