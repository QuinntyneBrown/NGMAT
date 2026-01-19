using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

internal sealed class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferencesEntity>
{
    public void Configure(EntityTypeBuilder<NotificationPreferencesEntity> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.WebhookUrl)
            .HasMaxLength(2000);

        builder.Property(p => p.WebhookSecret)
            .HasMaxLength(256);

        builder.Property(p => p.EmailSubscriptions)
            .HasMaxLength(4000);

        builder.Property(p => p.InAppSubscriptions)
            .HasMaxLength(4000);

        builder.Property(p => p.DigestFrequency)
            .HasMaxLength(50);

        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
