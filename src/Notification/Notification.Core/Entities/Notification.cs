using Notification.Core.Models;

namespace Notification.Core.Entities;

/// <summary>
/// Base notification entity for persistence
/// </summary>
public class NotificationEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? ActionUrl { get; private set; }
    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public string? Metadata { get; private set; }

    private NotificationEntity() { } // For EF Core

    public static NotificationEntity Create(
        string userId,
        NotificationType type,
        NotificationChannel channel,
        string title,
        string message,
        string? actionUrl = null,
        string? metadata = null)
    {
        return new NotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Channel = channel,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            Metadata = metadata,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = NotificationStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRead()
    {
        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    public void ResetForRetry()
    {
        Status = NotificationStatus.Pending;
        ErrorMessage = null;
    }
}

/// <summary>
/// Email notification entity
/// </summary>
public class EmailNotificationEntity
{
    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public NotificationEntity Notification { get; private set; } = null!;
    public string ToAddress { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsHtml { get; private set; }
    public string? CcAddresses { get; private set; }
    public string? BccAddresses { get; private set; }
    public string? TemplateId { get; private set; }
    public string? TemplateData { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private EmailNotificationEntity() { } // For EF Core

    public static EmailNotificationEntity Create(
        NotificationEntity notification,
        string toAddress,
        string subject,
        string body,
        bool isHtml = false,
        string? ccAddresses = null,
        string? bccAddresses = null,
        string? templateId = null,
        string? templateData = null)
    {
        return new EmailNotificationEntity
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            Notification = notification,
            ToAddress = toAddress,
            Subject = subject,
            Body = body,
            IsHtml = isHtml,
            CcAddresses = ccAddresses,
            BccAddresses = bccAddresses,
            TemplateId = templateId,
            TemplateData = templateData,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Webhook notification entity
/// </summary>
public class WebhookNotificationEntity
{
    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public NotificationEntity Notification { get; private set; } = null!;
    public string WebhookUrl { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string? Headers { get; private set; }
    public string? Secret { get; private set; }
    public int? ResponseStatusCode { get; private set; }
    public string? ResponseBody { get; private set; }
    public TimeSpan? ResponseTime { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private WebhookNotificationEntity() { } // For EF Core

    public static WebhookNotificationEntity Create(
        NotificationEntity notification,
        string webhookUrl,
        string eventType,
        string payload,
        string? headers = null,
        string? secret = null)
    {
        return new WebhookNotificationEntity
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            Notification = notification,
            WebhookUrl = webhookUrl,
            EventType = eventType,
            Payload = payload,
            Headers = headers,
            Secret = secret,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void RecordResponse(int statusCode, string? responseBody, TimeSpan responseTime)
    {
        ResponseStatusCode = statusCode;
        ResponseBody = responseBody;
        ResponseTime = responseTime;
    }
}

/// <summary>
/// In-app notification entity
/// </summary>
public class InAppNotificationEntity
{
    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public NotificationEntity Notification { get; private set; } = null!;
    public bool IsAcknowledged { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private InAppNotificationEntity() { } // For EF Core

    public static InAppNotificationEntity Create(NotificationEntity notification)
    {
        return new InAppNotificationEntity
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            Notification = notification,
            IsAcknowledged = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Acknowledge()
    {
        IsAcknowledged = true;
        AcknowledgedAt = DateTimeOffset.UtcNow;
    }
}
