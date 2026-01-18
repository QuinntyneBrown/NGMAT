using EventStore.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventStore.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the Event Store.
/// </summary>
public sealed class EventStoreDbContext : DbContext
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options)
    {
    }

    public DbSet<StoredEvent> Events => Set<StoredEvent>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<DeadLetterEvent> DeadLetters => Set<DeadLetterEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventStoreDbContext).Assembly);
    }
}
