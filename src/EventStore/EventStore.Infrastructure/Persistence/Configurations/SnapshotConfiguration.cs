using EventStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventStore.Infrastructure.Persistence.Configurations;

internal sealed class SnapshotConfiguration : IEntityTypeConfiguration<Snapshot>
{
    public void Configure(EntityTypeBuilder<Snapshot> builder)
    {
        builder.ToTable("Snapshots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.AggregateType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(s => s.Data)
            .IsRequired();

        // Index for querying latest snapshot
        builder.HasIndex(s => new { s.AggregateId, s.SequenceNumber });

        builder.HasIndex(s => s.Timestamp);
    }
}
