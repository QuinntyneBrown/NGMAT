using Microsoft.AspNetCore.Mvc;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Api.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications/webhook")
            .WithTags("Webhook Notifications");

        // Send webhook notification
        group.MapPost("/", SendWebhookAsync)
            .WithName("SendWebhook")
            .WithDescription("Send a webhook notification")
            .Produces<WebhookSendResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // Test webhook endpoint
        group.MapPost("/test", TestWebhookAsync)
            .WithName("TestWebhook")
            .WithDescription("Test a webhook URL")
            .Produces<WebhookTestResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SendWebhookAsync(
        [FromBody] SendWebhookRequest request,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WebhookUrl))
        {
            return Results.Problem(
                detail: "Webhook URL is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            return Results.Problem(
                detail: "Event type is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var webhookRequest = new WebhookNotificationRequest
        {
            UserId = request.UserId,
            WebhookUrl = request.WebhookUrl,
            EventType = request.EventType,
            Payload = request.Payload ?? new { },
            Headers = request.Headers ?? new Dictionary<string, string>(),
            Secret = request.Secret
        };

        var result = await notificationService.SendWebhookAsync(webhookRequest, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Accepted(
            value: new WebhookSendResponse
            {
                NotificationId = result.NotificationId,
                Status = "Accepted"
            });
    }

    private static async Task<IResult> TestWebhookAsync(
        [FromBody] TestWebhookRequest request,
        [FromServices] IWebhookService webhookService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WebhookUrl))
        {
            return Results.Problem(
                detail: "Webhook URL is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (!webhookService.ValidateWebhookUrl(request.WebhookUrl))
        {
            return Results.Problem(
                detail: "Invalid webhook URL format",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var testRequest = new WebhookNotificationRequest
        {
            UserId = "test",
            WebhookUrl = request.WebhookUrl,
            EventType = "test.ping",
            Payload = new
            {
                message = "This is a test webhook from NGMAT Notification Service",
                timestamp = DateTimeOffset.UtcNow
            },
            Secret = request.Secret
        };

        var result = await webhookService.SendAsync(testRequest, cancellationToken);

        return Results.Ok(new WebhookTestResponse
        {
            Success = result.Success,
            StatusCode = result.StatusCode,
            ResponseTime = result.ResponseTime,
            ErrorMessage = result.ErrorMessage
        });
    }
}

public sealed record SendWebhookRequest
{
    public string UserId { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public object? Payload { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public string? Secret { get; init; }
}

public sealed record TestWebhookRequest
{
    public string WebhookUrl { get; init; } = string.Empty;
    public string? Secret { get; init; }
}

public sealed record WebhookSendResponse
{
    public Guid NotificationId { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed record WebhookTestResponse
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public TimeSpan ResponseTime { get; init; }
    public string? ErrorMessage { get; init; }
}
