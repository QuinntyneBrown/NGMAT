using EventStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventStore.Infrastructure.Persistence.Configurations;

internal sealed class DeadLetterEventConfiguration : IEntityTypeConfiguration<DeadLetterEvent>
{
    public void Configure(EntityTypeBuilder<DeadLetterEvent> builder)
    {
        builder.ToTable("DeadLetterEvents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.ErrorMessage)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(d => d.SubscriptionId);

        builder.HasIndex(d => d.IsResolved);
    }
}
