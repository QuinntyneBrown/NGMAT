using EventStore.Core.Interfaces;
using EventStore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventStore.Infrastructure.Persistence;

internal sealed class EventStoreUnitOfWork : IEventStoreUnitOfWork
{
    private readonly EventStoreDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private IEventRepository? _events;
    private ISnapshotRepository? _snapshots;
    private ISubscriptionRepository? _subscriptions;
    private IDeadLetterRepository? _deadLetters;

    public EventStoreUnitOfWork(EventStoreDbContext context)
    {
        _context = context;
    }

    public IEventRepository Events => _events ??= new EventRepository(_context);
    public ISnapshotRepository Snapshots => _snapshots ??= new SnapshotRepository(_context);
    public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);
    public IDeadLetterRepository DeadLetters => _deadLetters ??= new DeadLetterRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
