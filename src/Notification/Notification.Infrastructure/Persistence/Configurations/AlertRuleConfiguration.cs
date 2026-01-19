using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

internal sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToTable("AlertRules");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Condition)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.ConditionOperator)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.ConditionValue)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.NotificationChannel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.NotificationTitle)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.NotificationMessage)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.IsEnabled);
        builder.HasIndex(a => new { a.IsEnabled, a.LastEvaluatedAt });
    }
}
