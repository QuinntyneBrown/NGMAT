using Notification.Core.Models;

namespace Notification.Core.Interfaces;

/// <summary>
/// Email sending service interface
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email notification
    /// </summary>
    Task<NotificationSendResult> SendAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template
    /// </summary>
    Task<NotificationSendResult> SendTemplatedAsync(
        string toAddress,
        string templateId,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an email address format
    /// </summary>
    bool ValidateEmailAddress(string email);
}
