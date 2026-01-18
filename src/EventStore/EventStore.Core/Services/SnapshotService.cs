using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Shared.Domain.Results;

namespace EventStore.Core.Services;

/// <summary>
/// Service for managing event snapshots.
/// </summary>
public sealed class SnapshotService
{
    private readonly IEventStoreUnitOfWork _unitOfWork;
    private readonly EventStoreOptions _options;

    public SnapshotService(IEventStoreUnitOfWork unitOfWork, EventStoreOptions options)
    {
        _unitOfWork = unitOfWork;
        _options = options;
    }

    public async Task<Result<Snapshot>> CreateSnapshotAsync(
        Guid aggregateId,
        string aggregateType,
        string stateData,
        int version = 1,
        CancellationToken cancellationToken = default)
    {
        // Get the latest sequence number for this aggregate
        var latestSequence = await _unitOfWork.Events.GetLatestSequenceNumberAsync(aggregateId, cancellationToken);

        var snapshot = Snapshot.Create(
            aggregateId,
            aggregateType,
            latestSequence,
            stateData,
            version);

        var saved = await _unitOfWork.Snapshots.SaveAsync(snapshot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saved;
    }

    public async Task<Result<Snapshot?>> GetLatestSnapshotAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _unitOfWork.Snapshots.GetLatestAsync(aggregateId, cancellationToken);
        return snapshot;
    }

    public async Task<Result<ReplayResult>> ReplayEventsAsync(
        Guid aggregateId,
        Func<string, string, string> applyEvent,
        CancellationToken cancellationToken = default)
    {
        // Get latest snapshot
        var snapshot = await _unitOfWork.Snapshots.GetLatestAsync(aggregateId, cancellationToken);

        string currentState;
        long fromSequence;

        if (snapshot != null)
        {
            currentState = snapshot.Data;
            fromSequence = snapshot.SequenceNumber + 1;
        }
        else
        {
            currentState = "{}";
            fromSequence = 1;
        }

        // Get events after the snapshot
        var events = await _unitOfWork.Events.GetByAggregateIdAsync(
            aggregateId,
            fromSequence,
            null,
            cancellationToken);

        // Apply each event
        foreach (var @event in events)
        {
            currentState = applyEvent(currentState, @event.Data);
        }

        return new ReplayResult
        {
            AggregateId = aggregateId,
            CurrentState = currentState,
            SequenceNumber = events.Count > 0 ? events.Max(e => e.SequenceNumber) : (snapshot?.SequenceNumber ?? 0),
            EventsApplied = events.Count,
            UsedSnapshot = snapshot != null
        };
    }

    public async Task<Result<int>> CleanupOldSnapshotsAsync(
        Guid aggregateId,
        int keepCount = 5,
        CancellationToken cancellationToken = default)
    {
        var snapshots = await _unitOfWork.Snapshots.GetAllAsync(aggregateId, cancellationToken);

        if (snapshots.Count <= keepCount)
        {
            return 0;
        }

        var cutoffDate = snapshots
            .OrderByDescending(s => s.Timestamp)
            .Skip(keepCount)
            .First()
            .Timestamp;

        var deleted = await _unitOfWork.Snapshots.DeleteOlderThanAsync(aggregateId, cutoffDate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return deleted;
    }
}

/// <summary>
/// Result of replaying events for an aggregate.
/// </summary>
public sealed class ReplayResult
{
    public Guid AggregateId { get; init; }
    public string CurrentState { get; init; } = string.Empty;
    public long SequenceNumber { get; init; }
    public int EventsApplied { get; init; }
    public bool UsedSnapshot { get; init; }
}
