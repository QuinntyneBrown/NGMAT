namespace Notification.Core.Models;

/// <summary>
/// Notification type
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Notification channel
/// </summary>
public enum NotificationChannel
{
    InApp,
    Email,
    Webhook
}

/// <summary>
/// Notification status
/// </summary>
public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}

/// <summary>
/// User notification
/// </summary>
public sealed class UserNotification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? SentAt { get; set; }
    public NotificationChannel Channel { get; init; } = NotificationChannel.InApp;
}

/// <summary>
/// Email notification request
/// </summary>
public sealed class EmailNotificationRequest
{
    public string UserId { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; }
    public List<string> Cc { get; init; } = new();
    public List<string> Bcc { get; init; } = new();
    public Dictionary<string, byte[]> Attachments { get; init; } = new();
}

/// <summary>
/// Webhook notification request
/// </summary>
public sealed class WebhookNotificationRequest
{
    public string UserId { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public object Payload { get; init; } = new { };
    public Dictionary<string, string> Headers { get; init; } = new();
    public string? Secret { get; init; }
}

/// <summary>
/// User notification preferences
/// </summary>
public sealed class NotificationPreferences
{
    public string UserId { get; init; } = string.Empty;
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool WebhookEnabled { get; set; }
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    public List<string> EmailSubscriptions { get; set; } = new()
    {
        "mission_complete",
        "propagation_complete",
        "conjunction_alert",
        "maneuver_scheduled"
    };
    public List<string> InAppSubscriptions { get; set; } = new()
    {
        "mission_complete",
        "propagation_complete",
        "conjunction_alert",
        "maneuver_scheduled",
        "script_execution_complete",
        "optimization_complete"
    };
    public bool QuietHoursEnabled { get; set; }
    public TimeOnly QuietHoursStart { get; set; } = new(22, 0);
    public TimeOnly QuietHoursEnd { get; set; } = new(7, 0);
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Notification send result
/// </summary>
public sealed class NotificationSendResult
{
    public bool Success { get; init; }
    public Guid NotificationId { get; init; }
    public string? ErrorMessage { get; init; }
    public NotificationChannel Channel { get; init; }
    public DateTime SentAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Webhook delivery result
/// </summary>
public sealed class WebhookDeliveryResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string? ResponseBody { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ResponseTime { get; init; }
}

/// <summary>
/// Notification query filter
/// </summary>
public sealed class NotificationFilter
{
    public string? UserId { get; init; }
    public NotificationType? Type { get; init; }
    public NotificationStatus? Status { get; init; }
    public NotificationChannel? Channel { get; init; }
    public bool? UnreadOnly { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; } = 50;
}

/// <summary>
/// Real-time notification message for connected clients
/// </summary>
public sealed class RealTimeNotification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
