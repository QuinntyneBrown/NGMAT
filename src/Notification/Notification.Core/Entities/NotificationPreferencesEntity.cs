namespace Notification.Core.Entities;

/// <summary>
/// User notification preferences entity for persistence
/// </summary>
public class NotificationPreferencesEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public bool EmailEnabled { get; private set; } = true;
    public bool InAppEnabled { get; private set; } = true;
    public bool WebhookEnabled { get; private set; }
    public string? WebhookUrl { get; private set; }
    public string? WebhookSecret { get; private set; }
    public string EmailSubscriptions { get; private set; } = string.Empty;
    public string InAppSubscriptions { get; private set; } = string.Empty;
    public bool QuietHoursEnabled { get; private set; }
    public TimeOnly QuietHoursStart { get; private set; } = new(22, 0);
    public TimeOnly QuietHoursEnd { get; private set; } = new(7, 0);
    public bool DigestEnabled { get; private set; }
    public string DigestFrequency { get; private set; } = "daily";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private NotificationPreferencesEntity() { } // For EF Core

    public static NotificationPreferencesEntity Create(string userId)
    {
        return new NotificationPreferencesEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            InAppEnabled = true,
            WebhookEnabled = false,
            EmailSubscriptions = "mission_complete,propagation_complete,conjunction_alert,maneuver_scheduled",
            InAppSubscriptions = "mission_complete,propagation_complete,conjunction_alert,maneuver_scheduled,script_execution_complete,optimization_complete",
            QuietHoursEnabled = false,
            DigestEnabled = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateEmailSettings(bool enabled, IEnumerable<string>? subscriptions = null)
    {
        EmailEnabled = enabled;
        if (subscriptions is not null)
        {
            EmailSubscriptions = string.Join(",", subscriptions);
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateInAppSettings(bool enabled, IEnumerable<string>? subscriptions = null)
    {
        InAppEnabled = enabled;
        if (subscriptions is not null)
        {
            InAppSubscriptions = string.Join(",", subscriptions);
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateWebhookSettings(bool enabled, string? webhookUrl = null, string? secret = null)
    {
        WebhookEnabled = enabled;
        if (webhookUrl is not null) WebhookUrl = webhookUrl;
        if (secret is not null) WebhookSecret = secret;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateQuietHours(bool enabled, TimeOnly? start = null, TimeOnly? end = null)
    {
        QuietHoursEnabled = enabled;
        if (start.HasValue) QuietHoursStart = start.Value;
        if (end.HasValue) QuietHoursEnd = end.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDigestSettings(bool enabled, string? frequency = null)
    {
        DigestEnabled = enabled;
        if (frequency is not null) DigestFrequency = frequency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public List<string> GetEmailSubscriptionsList() =>
        string.IsNullOrEmpty(EmailSubscriptions)
            ? new List<string>()
            : EmailSubscriptions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

    public List<string> GetInAppSubscriptionsList() =>
        string.IsNullOrEmpty(InAppSubscriptions)
            ? new List<string>()
            : InAppSubscriptions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

    public bool IsInQuietHours(TimeOnly currentTime)
    {
        if (!QuietHoursEnabled) return false;

        if (QuietHoursStart < QuietHoursEnd)
        {
            return currentTime >= QuietHoursStart && currentTime <= QuietHoursEnd;
        }
        else
        {
            return currentTime >= QuietHoursStart || currentTime <= QuietHoursEnd;
        }
    }
}
