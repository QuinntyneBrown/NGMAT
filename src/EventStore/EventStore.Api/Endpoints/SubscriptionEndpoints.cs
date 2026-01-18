using EventStore.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventStore.Api.Endpoints;

/// <summary>
/// Subscription management API endpoints.
/// </summary>
public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/events/subscriptions")
            .WithTags("Subscriptions");

        group.MapPost("/", CreateSubscriptionAsync)
            .WithName("CreateSubscription")
            .WithDescription("Create a new event subscription")
            .Produces<SubscriptionResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/", GetActiveSubscriptionsAsync)
            .WithName("GetActiveSubscriptions")
            .WithDescription("Get all active subscriptions")
            .Produces<List<SubscriptionResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetSubscriptionAsync)
            .WithName("GetSubscription")
            .WithDescription("Get subscription by ID")
            .Produces<SubscriptionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", ActivateSubscriptionAsync)
            .WithName("ActivateSubscription")
            .WithDescription("Activate a subscription")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateSubscriptionAsync)
            .WithName("DeactivateSubscription")
            .WithDescription("Deactivate a subscription")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteSubscriptionAsync)
            .WithName("DeleteSubscription")
            .WithDescription("Delete a subscription")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateSubscriptionAsync(
        [FromBody] CreateSubscriptionRequest request,
        [FromServices] SubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var result = await subscriptionService.CreateSubscriptionAsync(
            request.Name,
            request.EventTypes,
            request.CallbackUrl,
            request.AggregateTypes,
            cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Code == "Conflict"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            return Results.Problem(detail: result.Error.Message, statusCode: statusCode);
        }

        return Results.Created($"/api/v1/events/subscriptions/{result.Value.Id}", ToResponse(result.Value));
    }

    private static async Task<IResult> GetActiveSubscriptionsAsync(
        [FromServices] SubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var result = await subscriptionService.GetActiveSubscriptionsAsync(cancellationToken);
        return Results.Ok(result.Value.Select(ToResponse).ToList());
    }

    private static async Task<IResult> GetSubscriptionAsync(
        Guid id,
        [FromServices] SubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var result = await subscriptionService.GetSubscriptionAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> ActivateSubscriptionAsync(
        Guid id,
        [FromServices] SubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var result = await subscriptionService.ActivateSubscriptionAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateSubscriptionAsync(
        Guid id,
        [FromServices] SubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var result = await subscriptionService.DeactivateSubscriptionAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteSubscriptionAsync(
        Guid id,
        [FromServices] SubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var result = await subscriptionService.DeleteSubscriptionAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound);
        }

        return Results.NoContent();
    }

    private static SubscriptionResponse ToResponse(EventStore.Core.Entities.Subscription s)
    {
        return new SubscriptionResponse
        {
            Id = s.Id,
            Name = s.Name,
            EventTypes = s.GetEventTypes().ToList(),
            AggregateTypes = s.GetAggregateTypes()?.ToList(),
            CallbackUrl = s.CallbackUrl,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            LastDeliveryAt = s.LastDeliveryAt,
            LastDeliveredSequence = s.LastDeliveredSequence,
            FailureCount = s.FailureCount
        };
    }
}

// Request DTOs
public sealed record CreateSubscriptionRequest(
    string Name,
    List<string> EventTypes,
    string CallbackUrl,
    List<string>? AggregateTypes = null);

// Response DTOs
public sealed class SubscriptionResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public List<string> EventTypes { get; init; } = new();
    public List<string>? AggregateTypes { get; init; }
    public string CallbackUrl { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastDeliveryAt { get; init; }
    public long LastDeliveredSequence { get; init; }
    public int FailureCount { get; init; }
}
