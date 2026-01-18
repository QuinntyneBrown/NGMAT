using EventStore.Core.Entities;

namespace EventStore.Core.Interfaces;

/// <summary>
/// Repository interface for stored events.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Appends an event to the store.
    /// </summary>
    Task<StoredEvent> AppendAsync(StoredEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends multiple events to the store.
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> AppendBatchAsync(IEnumerable<StoredEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event by ID.
    /// </summary>
    Task<StoredEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events by aggregate ID.
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetByAggregateIdAsync(
        Guid aggregateId,
        long? fromSequence = null,
        long? toSequence = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events by event type.
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetByEventTypeAsync(
        string eventType,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events with filtering options.
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> QueryAsync(
        Guid? aggregateId = null,
        string? aggregateType = null,
        string? eventType = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        Guid? correlationId = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest sequence number for an aggregate.
    /// </summary>
    Task<long> GetLatestSequenceNumberAsync(Guid aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of events matching the filter.
    /// </summary>
    Task<int> GetCountAsync(
        Guid? aggregateId = null,
        string? aggregateType = null,
        string? eventType = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);
}
