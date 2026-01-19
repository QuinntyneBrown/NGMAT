using Microsoft.AspNetCore.Mvc;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications")
            .WithTags("Notifications");

        // Get notifications for user
        group.MapGet("/{userId}", GetNotificationsAsync)
            .WithName("GetNotifications")
            .WithDescription("Get notifications for a user")
            .Produces<IReadOnlyList<NotificationResponse>>(StatusCodes.Status200OK);

        // Get notification by ID
        group.MapGet("/details/{id:guid}", GetNotificationByIdAsync)
            .WithName("GetNotificationById")
            .WithDescription("Get a specific notification by ID")
            .Produces<NotificationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Get unread count
        group.MapGet("/{userId}/unread-count", GetUnreadCountAsync)
            .WithName("GetUnreadCount")
            .WithDescription("Get unread notification count for a user")
            .Produces<UnreadCountResponse>(StatusCodes.Status200OK);

        // Mark notification as read
        group.MapPut("/{id:guid}/read", MarkAsReadAsync)
            .WithName("MarkAsRead")
            .WithDescription("Mark a notification as read")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Mark all as read
        group.MapPut("/{userId}/read-all", MarkAllAsReadAsync)
            .WithName("MarkAllAsRead")
            .WithDescription("Mark all notifications as read for a user")
            .Produces(StatusCodes.Status204NoContent);

        // Delete notification
        group.MapDelete("/{id:guid}", DeleteNotificationAsync)
            .WithName("DeleteNotification")
            .WithDescription("Delete a notification")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetNotificationsAsync(
        [FromRoute] string userId,
        [FromQuery] NotificationType? type,
        [FromQuery] NotificationStatus? status,
        [FromQuery] NotificationChannel? channel,
        [FromQuery] bool? unreadOnly,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var filter = new NotificationFilter
        {
            Type = type,
            Status = status,
            Channel = channel,
            UnreadOnly = unreadOnly,
            Skip = skip,
            Take = take
        };

        var notifications = await notificationService.GetNotificationsAsync(userId, filter, cancellationToken);

        var response = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            UserId = n.UserId,
            Type = n.Type,
            Channel = n.Channel,
            Title = n.Title,
            Message = n.Message,
            ActionUrl = n.ActionUrl,
            Status = n.Status,
            CreatedAt = n.CreatedAt,
            SentAt = n.SentAt,
            ReadAt = n.ReadAt
        }).ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetNotificationByIdAsync(
        [FromRoute] Guid id,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var notification = await notificationService.GetNotificationByIdAsync(id, cancellationToken);

        if (notification is null)
            return Results.NotFound();

        return Results.Ok(new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Channel = notification.Channel,
            Title = notification.Title,
            Message = notification.Message,
            ActionUrl = notification.ActionUrl,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            ReadAt = notification.ReadAt
        });
    }

    private static async Task<IResult> GetUnreadCountAsync(
        [FromRoute] string userId,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var count = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Results.Ok(new UnreadCountResponse { Count = count });
    }

    private static async Task<IResult> MarkAsReadAsync(
        [FromRoute] Guid id,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var notification = await notificationService.GetNotificationByIdAsync(id, cancellationToken);
        if (notification is null)
            return Results.NotFound();

        await notificationService.MarkAsReadAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> MarkAllAsReadAsync(
        [FromRoute] string userId,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteNotificationAsync(
        [FromRoute] Guid id,
        [FromServices] INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var notification = await notificationService.GetNotificationByIdAsync(id, cancellationToken);
        if (notification is null)
            return Results.NotFound();

        await notificationService.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record NotificationResponse
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public NotificationStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? SentAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}

public sealed record UnreadCountResponse
{
    public int Count { get; init; }
}
