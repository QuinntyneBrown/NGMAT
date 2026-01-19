using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

internal sealed class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(2000);

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(n => n.Metadata)
            .HasMaxLength(8000);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Channel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => new { n.UserId, n.Status });
    }
}

internal sealed class EmailNotificationEntityConfiguration : IEntityTypeConfiguration<EmailNotificationEntity>
{
    public void Configure(EntityTypeBuilder<EmailNotificationEntity> builder)
    {
        builder.ToTable("EmailNotifications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ToAddress)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Body)
            .IsRequired();

        builder.Property(e => e.CcAddresses)
            .HasMaxLength(2000);

        builder.Property(e => e.BccAddresses)
            .HasMaxLength(2000);

        builder.Property(e => e.TemplateId)
            .HasMaxLength(256);

        builder.Property(e => e.TemplateData)
            .HasMaxLength(8000);

        builder.HasOne(e => e.Notification)
            .WithMany()
            .HasForeignKey(e => e.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.NotificationId);
    }
}

internal sealed class WebhookNotificationEntityConfiguration : IEntityTypeConfiguration<WebhookNotificationEntity>
{
    public void Configure(EntityTypeBuilder<WebhookNotificationEntity> builder)
    {
        builder.ToTable("WebhookNotifications");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WebhookUrl)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(w => w.EventType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(w => w.Payload)
            .IsRequired();

        builder.Property(w => w.Headers)
            .HasMaxLength(4000);

        builder.Property(w => w.Secret)
            .HasMaxLength(256);

        builder.Property(w => w.ResponseBody)
            .HasMaxLength(8000);

        builder.HasOne(w => w.Notification)
            .WithMany()
            .HasForeignKey(w => w.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.NotificationId);
    }
}

internal sealed class InAppNotificationEntityConfiguration : IEntityTypeConfiguration<InAppNotificationEntity>
{
    public void Configure(EntityTypeBuilder<InAppNotificationEntity> builder)
    {
        builder.ToTable("InAppNotifications");

        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.Notification)
            .WithMany()
            .HasForeignKey(i => i.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.NotificationId);
    }
}
