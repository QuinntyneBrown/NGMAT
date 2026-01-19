namespace Notification.Infrastructure.Services;

/// <summary>
/// Webhook service configuration options
/// </summary>
public sealed class WebhookOptions
{
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public string SignatureHeaderName { get; set; } = "X-Webhook-Signature";
    public string TimestampHeaderName { get; set; } = "X-Webhook-Timestamp";
    public string EventTypeHeaderName { get; set; } = "X-Webhook-Event";
}
