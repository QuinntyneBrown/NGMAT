using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using Polly;
using Polly.Retry;

namespace Notification.Infrastructure.Services;

internal sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public EmailService(EmailOptions options, ILogger<EmailService> logger)
    {
        _options = options;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                _options.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(_options.RetryDelaySeconds * Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Email send attempt {RetryCount} failed. Retrying in {RetryDelay}s",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    public async Task<NotificationSendResult> SendAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateEmailAddress(request.To))
        {
            return new NotificationSendResult
            {
                Success = false,
                NotificationId = Guid.Empty,
                ErrorMessage = "Invalid email address format",
                Channel = NotificationChannel.Email
            };
        }

        var notificationId = Guid.NewGuid();

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await SendEmailInternalAsync(request, cancellationToken);
            });

            _logger.LogInformation(
                "Email sent successfully to {ToAddress} with subject: {Subject}",
                request.To,
                request.Subject);

            return new NotificationSendResult
            {
                Success = true,
                NotificationId = notificationId,
                Channel = NotificationChannel.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {ToAddress} after {MaxRetries} attempts",
                request.To,
                _options.MaxRetries);

            return new NotificationSendResult
            {
                Success = false,
                NotificationId = notificationId,
                ErrorMessage = ex.Message,
                Channel = NotificationChannel.Email
            };
        }
    }

    public async Task<NotificationSendResult> SendTemplatedAsync(
        string toAddress,
        string templateId,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would fetch the template from the repository
        // and render it with the provided data
        var request = new EmailNotificationRequest
        {
            To = toAddress,
            Subject = $"Notification - {templateId}",
            Body = string.Join("<br>", templateData.Select(kv => $"{kv.Key}: {kv.Value}")),
            IsHtml = true
        };

        return await SendAsync(request, cancellationToken);
    }

    public bool ValidateEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email);
    }

    private async Task SendEmailInternalAsync(
        EmailNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(request.To));

        if (request.Cc.Count > 0)
        {
            foreach (var cc in request.Cc)
            {
                message.Cc.Add(MailboxAddress.Parse(cc));
            }
        }

        if (request.Bcc.Count > 0)
        {
            foreach (var bcc in request.Bcc)
            {
                message.Bcc.Add(MailboxAddress.Parse(bcc));
            }
        }

        message.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder();

        if (request.IsHtml)
        {
            bodyBuilder.HtmlBody = request.Body;
        }
        else
        {
            bodyBuilder.TextBody = request.Body;
        }

        if (request.Attachments.Count > 0)
        {
            foreach (var (fileName, content) in request.Attachments)
            {
                bodyBuilder.Attachments.Add(fileName, content);
            }
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        var secureSocketOptions = _options.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(
            _options.SmtpHost,
            _options.SmtpPort,
            secureSocketOptions,
            cancellationToken);

        if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
