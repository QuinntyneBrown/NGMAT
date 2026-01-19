using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;

namespace Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();
    public DbSet<EmailNotificationEntity> EmailNotifications => Set<EmailNotificationEntity>();
    public DbSet<WebhookNotificationEntity> WebhookNotifications => Set<WebhookNotificationEntity>();
    public DbSet<InAppNotificationEntity> InAppNotifications => Set<InAppNotificationEntity>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<NotificationPreferencesEntity> Preferences => Set<NotificationPreferencesEntity>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
