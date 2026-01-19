using System.Text.Json;
using Microsoft.Extensions.Logging;
using Notification.Core.Entities;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Infrastructure.Services;

internal sealed class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IWebhookService _webhookService;
    private readonly IRealTimeNotificationService _realTimeService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IWebhookService webhookService,
        IRealTimeNotificationService realTimeService,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _webhookService = webhookService;
        _realTimeService = realTimeService;
        _logger = logger;
    }

    public async Task<NotificationSendResult> SendEmailAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check user preferences
        var preferences = await _unitOfWork.Preferences.GetByUserIdAsync(request.UserId, cancellationToken);
        if (preferences is not null && !preferences.EmailEnabled)
        {
            _logger.LogInformation("Email notifications disabled for user {UserId}", request.UserId);
            return new NotificationSendResult
            {
                Success = false,
                NotificationId = Guid.Empty,
                ErrorMessage = "Email notifications disabled for this user",
                Channel = NotificationChannel.Email
            };
        }

        // Check quiet hours
        if (preferences?.IsInQuietHours(TimeOnly.FromDateTime(DateTime.UtcNow)) == true)
        {
            _logger.LogInformation("User {UserId} is in quiet hours, queuing notification", request.UserId);
            // Queue for later delivery - for now we just skip
        }

        // Create notification entity for tracking
        var notification = NotificationEntity.Create(
            request.UserId,
            NotificationType.Info,
            NotificationChannel.Email,
            request.Subject,
            request.Body,
            metadata: JsonSerializer.Serialize(new { To = request.To }));

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);

        // Create email notification details
        var emailNotification = EmailNotificationEntity.Create(
            notification,
            request.To,
            request.Subject,
            request.Body,
            request.IsHtml,
            request.Cc.Count > 0 ? string.Join(",", request.Cc) : null,
            request.Bcc.Count > 0 ? string.Join(",", request.Bcc) : null);

        // Send email
        var result = await _emailService.SendAsync(request, cancellationToken);

        if (result.Success)
        {
            notification.MarkAsSent();
        }
        else
        {
            notification.MarkAsFailed(result.ErrorMessage ?? "Unknown error");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new NotificationSendResult
        {
            Success = result.Success,
            NotificationId = notification.Id,
            ErrorMessage = result.ErrorMessage,
            Channel = NotificationChannel.Email
        };
    }

    public async Task<NotificationSendResult> SendWebhookAsync(
        WebhookNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check user preferences
        var preferences = await _unitOfWork.Preferences.GetByUserIdAsync(request.UserId, cancellationToken);
        if (preferences is not null && !preferences.WebhookEnabled)
        {
            _logger.LogInformation("Webhook notifications disabled for user {UserId}", request.UserId);
            return new NotificationSendResult
            {
                Success = false,
                NotificationId = Guid.Empty,
                ErrorMessage = "Webhook notifications disabled for this user",
                Channel = NotificationChannel.Webhook
            };
        }

        // Create notification entity for tracking
        var notification = NotificationEntity.Create(
            request.UserId,
            NotificationType.Info,
            NotificationChannel.Webhook,
            request.EventType,
            JsonSerializer.Serialize(request.Payload),
            metadata: JsonSerializer.Serialize(new { Url = request.WebhookUrl }));

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);

        // Send webhook
        var result = await _webhookService.SendAsync(request, cancellationToken);

        if (result.Success)
        {
            notification.MarkAsSent();
        }
        else
        {
            notification.MarkAsFailed(result.ErrorMessage ?? "Unknown error");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new NotificationSendResult
        {
            Success = result.Success,
            NotificationId = notification.Id,
            ErrorMessage = result.ErrorMessage,
            Channel = NotificationChannel.Webhook
        };
    }

    public async Task<NotificationSendResult> SendInAppAsync(
        UserNotification userNotification,
        CancellationToken cancellationToken = default)
    {
        // Check user preferences
        var preferences = await _unitOfWork.Preferences.GetByUserIdAsync(userNotification.UserId, cancellationToken);
        if (preferences is not null && !preferences.InAppEnabled)
        {
            _logger.LogInformation("In-app notifications disabled for user {UserId}", userNotification.UserId);
            return new NotificationSendResult
            {
                Success = false,
                NotificationId = Guid.Empty,
                ErrorMessage = "In-app notifications disabled for this user",
                Channel = NotificationChannel.InApp
            };
        }

        // Create notification entity for tracking
        var notification = NotificationEntity.Create(
            userNotification.UserId,
            userNotification.Type,
            NotificationChannel.InApp,
            userNotification.Title,
            userNotification.Message,
            userNotification.ActionUrl,
            userNotification.Metadata.Count > 0 ? JsonSerializer.Serialize(userNotification.Metadata) : null);

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send real-time notification
        var realTimeNotification = new RealTimeNotification
        {
            Id = notification.Id,
            UserId = userNotification.UserId,
            Type = userNotification.Type,
            Title = userNotification.Title,
            Message = userNotification.Message,
            ActionUrl = userNotification.ActionUrl
        };

        await _realTimeService.SendToUserAsync(userNotification.UserId, realTimeNotification, cancellationToken);

        notification.MarkAsSent();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new NotificationSendResult
        {
            Success = true,
            NotificationId = notification.Id,
            Channel = NotificationChannel.InApp
        };
    }

    public async Task<IReadOnlyList<NotificationEntity>> GetNotificationsAsync(
        string userId,
        NotificationFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Notifications.GetByUserAsync(userId, filter, cancellationToken);
    }

    public async Task<NotificationEntity?> GetNotificationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.DeleteAsync(notificationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RetryFailedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var failedNotifications = await _unitOfWork.Notifications.GetFailedForRetryAsync(3, cancellationToken);

        _logger.LogInformation("Found {Count} failed notifications to retry", failedNotifications.Count);

        foreach (var notification in failedNotifications)
        {
            notification.ResetForRetry();

            // Re-send based on channel
            // Note: In a real implementation, we'd fetch the original request details
            // from the EmailNotification or WebhookNotification tables

            _logger.LogInformation(
                "Retrying notification {NotificationId}, attempt {RetryCount}",
                notification.Id,
                notification.RetryCount + 1);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
