using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventStore.Infrastructure.Persistence.Repositories;

internal sealed class DeadLetterRepository : IDeadLetterRepository
{
    private readonly EventStoreDbContext _context;

    public DeadLetterRepository(EventStoreDbContext context)
    {
        _context = context;
    }

    public async Task<DeadLetterEvent> AddAsync(DeadLetterEvent deadLetter, CancellationToken cancellationToken = default)
    {
        await _context.DeadLetters.AddAsync(deadLetter, cancellationToken);
        return deadLetter;
    }

    public async Task<IReadOnlyList<DeadLetterEvent>> GetBySubscriptionAsync(
        Guid subscriptionId,
        bool includeResolved = false,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DeadLetters
            .Where(d => d.SubscriptionId == subscriptionId);

        if (!includeResolved)
        {
            query = query.Where(d => !d.IsResolved);
        }

        return await query
            .OrderByDescending(d => d.LastAttemptAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeadLetterEvent>> GetUnresolvedAsync(
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.DeadLetters
            .Where(d => !d.IsResolved)
            .OrderByDescending(d => d.LastAttemptAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task ResolveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deadLetter = await _context.DeadLetters.FindAsync(new object[] { id }, cancellationToken);
        if (deadLetter != null)
        {
            deadLetter.MarkResolved();
        }
    }

    public async Task<int> GetUnresolvedCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeadLetters
            .Where(d => !d.IsResolved)
            .CountAsync(cancellationToken);
    }
}
