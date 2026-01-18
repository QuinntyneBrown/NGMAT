using EventStore.Core.Entities;
using EventStore.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventStore.Infrastructure.Persistence.Repositories;

internal sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly EventStoreDbContext _context;

    public SubscriptionRepository(EventStoreDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        return subscription;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Subscription?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.IsActive && s.EventTypes.Contains(eventType))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetDueForRetryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.Subscriptions
            .Where(s => s.IsActive && s.NextRetryAt != null && s.NextRetryAt <= now)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
        if (subscription != null)
        {
            _context.Subscriptions.Remove(subscription);
        }
    }
}
