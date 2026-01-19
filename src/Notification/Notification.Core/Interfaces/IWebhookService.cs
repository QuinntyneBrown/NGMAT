using Notification.Core.Models;

namespace Notification.Core.Interfaces;

/// <summary>
/// Webhook delivery service interface
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook notification
    /// </summary>
    Task<WebhookDeliveryResult> SendAsync(WebhookNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates HMAC signature for webhook payload
    /// </summary>
    string GenerateSignature(string payload, string secret);

    /// <summary>
    /// Validates webhook URL
    /// </summary>
    bool ValidateWebhookUrl(string url);
}
