using System.Security.Cryptography;
using System.Text;
using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace EventStore.Core.Services;

/// <summary>
/// Service for storing and retrieving events.
/// </summary>
public sealed class EventStoreService
{
    private readonly IEventStoreUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly EventStoreOptions _options;

    public EventStoreService(
        IEventStoreUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        EventStoreOptions options)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _options = options;
    }

    public async Task<Result<StoredEvent>> AppendEventAsync(
        string eventType,
        Guid aggregateId,
        string aggregateType,
        string data,
        Guid? userId = null,
        Guid? correlationId = null,
        Guid? causationId = null,
        int version = 1,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // Get next sequence number
        var lastSequence = await _unitOfWork.Events.GetLatestSequenceNumberAsync(aggregateId, cancellationToken);
        var sequenceNumber = lastSequence + 1;

        // Create stored event
        var storedEvent = StoredEvent.Create(
            eventType,
            aggregateId,
            aggregateType,
            sequenceNumber,
            data,
            userId,
            correlationId,
            causationId,
            version,
            metadata);

        // Generate hash for audit trail
        if (_options.EnableAuditHashing)
        {
            var hash = GenerateHash(storedEvent);
            storedEvent.SetHash(hash);
        }

        // Persist event
        var saved = await _unitOfWork.Events.AppendAsync(storedEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Check if snapshot is needed
        if (_options.SnapshotInterval > 0 && sequenceNumber % _options.SnapshotInterval == 0)
        {
            // Snapshot creation would be handled by the snapshot service
        }

        return saved;
    }

    public async Task<Result<IReadOnlyList<StoredEvent>>> AppendEventsAsync(
        IEnumerable<AppendEventRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var events = new List<StoredEvent>();

        foreach (var request in requests)
        {
            var lastSequence = await _unitOfWork.Events.GetLatestSequenceNumberAsync(request.AggregateId, cancellationToken);
            var sequenceNumber = lastSequence + 1;

            var storedEvent = StoredEvent.Create(
                request.EventType,
                request.AggregateId,
                request.AggregateType,
                sequenceNumber,
                request.Data,
                request.UserId,
                request.CorrelationId,
                request.CausationId,
                request.Version,
                request.Metadata);

            if (_options.EnableAuditHashing)
            {
                var hash = GenerateHash(storedEvent);
                storedEvent.SetHash(hash);
            }

            events.Add(storedEvent);
        }

        var saved = await _unitOfWork.Events.AppendBatchAsync(events, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saved.ToList();
    }

    public async Task<Result<IReadOnlyList<StoredEvent>>> GetEventsAsync(
        Guid? aggregateId = null,
        string? aggregateType = null,
        string? eventType = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        Guid? correlationId = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.QueryAsync(
            aggregateId,
            aggregateType,
            eventType,
            fromDate,
            toDate,
            correlationId,
            skip,
            take,
            cancellationToken);

        return events.ToList();
    }

    public async Task<Result<IReadOnlyList<StoredEvent>>> GetEventsByAggregateAsync(
        Guid aggregateId,
        long? fromSequence = null,
        long? toSequence = null,
        CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetByAggregateIdAsync(
            aggregateId,
            fromSequence,
            toSequence,
            cancellationToken);

        return events.ToList();
    }

    public async Task<Result<int>> GetEventCountAsync(
        Guid? aggregateId = null,
        string? aggregateType = null,
        string? eventType = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.Events.GetCountAsync(
            aggregateId,
            aggregateType,
            eventType,
            fromDate,
            toDate,
            cancellationToken);

        return count;
    }

    private string GenerateHash(StoredEvent @event)
    {
        var content = $"{@event.EventType}|{@event.AggregateId}|{@event.SequenceNumber}|{@event.Data}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Request for appending an event.
/// </summary>
public sealed record AppendEventRequest(
    string EventType,
    Guid AggregateId,
    string AggregateType,
    string Data,
    Guid? UserId = null,
    Guid? CorrelationId = null,
    Guid? CausationId = null,
    int Version = 1,
    string? Metadata = null);

/// <summary>
/// Configuration options for the event store.
/// </summary>
public sealed class EventStoreOptions
{
    public bool EnableAuditHashing { get; set; } = true;
    public int SnapshotInterval { get; set; } = 100;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public double RetryBackoffMultiplier { get; set; } = 2.0;
}
