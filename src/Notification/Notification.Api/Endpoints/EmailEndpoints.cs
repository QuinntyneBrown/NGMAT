using Microsoft.AspNetCore.Mvc;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Api.Endpoints;

public static class EmailEndpoints
{
    public static void MapEmailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications/email")
            .WithTags("Email Notifications");

        // Send email notification
        group.MapPost("/", SendEmailAsync)
            .WithName("SendEmail")
            .WithDescription("Send an email notification")
            .Produces<SendNotificationResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // Send templated email
        group.MapPost("/template", SendTemplatedEmailAsync)
            .WithName("SendTemplatedEmail")
            .WithDescription("Send an email using a template")
            .Produces<SendNotificationResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SendEmailAsync(
        [FromBody] SendEmailRequest request,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.To))
        {
            return Results.Problem(
                detail: "Recipient email address is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            return Results.Problem(
                detail: "Email subject is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var emailRequest = new EmailNotificationRequest
        {
            UserId = request.UserId,
            To = request.To,
            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,
            Cc = request.Cc ?? new List<string>(),
            Bcc = request.Bcc ?? new List<string>()
        };

        var result = await notificationService.SendEmailAsync(emailRequest, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Accepted(
            value: new SendNotificationResponse
            {
                NotificationId = result.NotificationId,
                Channel = result.Channel.ToString(),
                Status = "Accepted"
            });
    }

    private static async Task<IResult> SendTemplatedEmailAsync(
        [FromBody] SendTemplatedEmailRequest request,
        [FromServices] IEmailService emailService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.To))
        {
            return Results.Problem(
                detail: "Recipient email address is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.TemplateId))
        {
            return Results.Problem(
                detail: "Template ID is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await emailService.SendTemplatedAsync(
            request.To,
            request.TemplateId,
            request.TemplateData ?? new Dictionary<string, string>(),
            cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Accepted(
            value: new SendNotificationResponse
            {
                NotificationId = result.NotificationId,
                Channel = result.Channel.ToString(),
                Status = "Accepted"
            });
    }
}

public sealed record SendEmailRequest
{
    public string UserId { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; }
    public List<string>? Cc { get; init; }
    public List<string>? Bcc { get; init; }
}

public sealed record SendTemplatedEmailRequest
{
    public string To { get; init; } = string.Empty;
    public string TemplateId { get; init; } = string.Empty;
    public Dictionary<string, string>? TemplateData { get; init; }
}

public sealed record SendNotificationResponse
{
    public Guid NotificationId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
