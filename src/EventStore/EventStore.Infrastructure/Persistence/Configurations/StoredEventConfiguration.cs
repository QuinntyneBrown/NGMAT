using EventStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventStore.Infrastructure.Persistence.Configurations;

internal sealed class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.AggregateType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Data)
            .IsRequired();

        builder.Property(e => e.Metadata)
            .HasMaxLength(4000);

        builder.Property(e => e.Hash)
            .HasMaxLength(128);

        // Index for querying by aggregate
        builder.HasIndex(e => new { e.AggregateId, e.SequenceNumber })
            .IsUnique();

        // Index for querying by event type
        builder.HasIndex(e => e.EventType);

        // Index for querying by timestamp
        builder.HasIndex(e => e.Timestamp);

        // Index for querying by correlation ID
        builder.HasIndex(e => e.CorrelationId);
    }
}
