using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventStore.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository : IEventRepository
{
    private readonly EventStoreDbContext _context;

    public EventRepository(EventStoreDbContext context)
    {
        _context = context;
    }

    public async Task<StoredEvent> AppendAsync(StoredEvent @event, CancellationToken cancellationToken = default)
    {
        await _context.Events.AddAsync(@event, cancellationToken);
        return @event;
    }

    public async Task<IReadOnlyList<StoredEvent>> AppendBatchAsync(IEnumerable<StoredEvent> events, CancellationToken cancellationToken = default)
    {
        var eventList = events.ToList();
        await _context.Events.AddRangeAsync(eventList, cancellationToken);
        return eventList;
    }

    public async Task<StoredEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> GetByAggregateIdAsync(
        Guid aggregateId,
        long? fromSequence = null,
        long? toSequence = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .Where(e => e.AggregateId == aggregateId);

        if (fromSequence.HasValue)
        {
            query = query.Where(e => e.SequenceNumber >= fromSequence.Value);
        }

        if (toSequence.HasValue)
        {
            query = query.Where(e => e.SequenceNumber <= toSequence.Value);
        }

        return await query
            .OrderBy(e => e.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> GetByEventTypeAsync(
        string eventType,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .Where(e => e.EventType == eventType);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredEvent>> QueryAsync(
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
        var query = _context.Events.AsQueryable();

        if (aggregateId.HasValue)
        {
            query = query.Where(e => e.AggregateId == aggregateId.Value);
        }

        if (!string.IsNullOrEmpty(aggregateType))
        {
            query = query.Where(e => e.AggregateType == aggregateType);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        if (correlationId.HasValue)
        {
            query = query.Where(e => e.CorrelationId == correlationId.Value);
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetLatestSequenceNumberAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var maxSequence = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .MaxAsync(e => (long?)e.SequenceNumber, cancellationToken);

        return maxSequence ?? 0;
    }

    public async Task<int> GetCountAsync(
        Guid? aggregateId = null,
        string? aggregateType = null,
        string? eventType = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events.AsQueryable();

        if (aggregateId.HasValue)
        {
            query = query.Where(e => e.AggregateId == aggregateId.Value);
        }

        if (!string.IsNullOrEmpty(aggregateType))
        {
            query = query.Where(e => e.AggregateType == aggregateType);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
