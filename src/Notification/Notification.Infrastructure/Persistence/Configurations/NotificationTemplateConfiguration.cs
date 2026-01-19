using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

internal sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Body)
            .IsRequired();

        builder.Property(t => t.Channel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Name).IsUnique();
        builder.HasIndex(t => t.Channel);
        builder.HasIndex(t => t.IsActive);
    }
}
