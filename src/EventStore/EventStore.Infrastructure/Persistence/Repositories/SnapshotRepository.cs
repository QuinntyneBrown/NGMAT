using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventStore.Infrastructure.Persistence.Repositories;

internal sealed class SnapshotRepository : ISnapshotRepository
{
    private readonly EventStoreDbContext _context;

    public SnapshotRepository(EventStoreDbContext context)
    {
        _context = context;
    }

    public async Task<Snapshot> SaveAsync(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _context.Snapshots.AddAsync(snapshot, cancellationToken);
        return snapshot;
    }

    public async Task<Snapshot?> GetLatestAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        return await _context.Snapshots
            .Where(s => s.AggregateId == aggregateId)
            .OrderByDescending(s => s.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Snapshot>> GetAllAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        return await _context.Snapshots
            .Where(s => s.AggregateId == aggregateId)
            .OrderByDescending(s => s.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(Guid aggregateId, DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        return await _context.Snapshots
            .Where(s => s.AggregateId == aggregateId && s.Timestamp < date)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
