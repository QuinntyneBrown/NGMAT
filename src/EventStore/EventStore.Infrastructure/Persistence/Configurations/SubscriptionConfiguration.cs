using EventStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventStore.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.Property(s => s.EventTypes)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.AggregateTypes)
            .HasMaxLength(2000);

        builder.Property(s => s.CallbackUrl)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(s => s.IsActive);

        builder.HasIndex(s => s.NextRetryAt);
    }
}
