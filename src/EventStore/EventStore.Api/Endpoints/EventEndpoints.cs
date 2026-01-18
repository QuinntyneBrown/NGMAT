using EventStore.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventStore.Api.Endpoints;

/// <summary>
/// Event store API endpoints.
/// </summary>
public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/events")
            .WithTags("Events");

        group.MapPost("/", AppendEventAsync)
            .WithName("AppendEvent")
            .WithDescription("Append a new event to the store")
            .Produces<StoredEventResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/batch", AppendEventsAsync)
            .WithName("AppendEventsBatch")
            .WithDescription("Append multiple events to the store")
            .Produces<List<StoredEventResponse>>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/", QueryEventsAsync)
            .WithName("QueryEvents")
            .WithDescription("Query events with filters")
            .Produces<PagedEventsResponse>(StatusCodes.Status200OK);

        group.MapGet("/{aggregateId:guid}", GetEventsByAggregateAsync)
            .WithName("GetEventsByAggregate")
            .WithDescription("Get all events for an aggregate")
            .Produces<List<StoredEventResponse>>(StatusCodes.Status200OK);

        group.MapPost("/replay/{aggregateId:guid}", ReplayEventsAsync)
            .WithName("ReplayEvents")
            .WithDescription("Replay events to reconstruct aggregate state")
            .Produces<ReplayResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/snapshot/{aggregateId:guid}", CreateSnapshotAsync)
            .WithName("CreateSnapshot")
            .WithDescription("Create a snapshot for an aggregate")
            .Produces<SnapshotResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/audit", QueryAuditTrailAsync)
            .WithName("QueryAuditTrail")
            .WithDescription("Query audit trail")
            .Produces<PagedEventsResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> AppendEventAsync(
        [FromBody] AppendEventRequest request,
        [FromServices] EventStoreService eventStoreService,
        CancellationToken cancellationToken)
    {
        var result = await eventStoreService.AppendEventAsync(
            request.EventType,
            request.AggregateId,
            request.AggregateType,
            request.Data,
            request.UserId,
            request.CorrelationId,
            request.CausationId,
            request.Version,
            request.Metadata,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Created($"/api/v1/events/{result.Value.Id}", ToResponse(result.Value));
    }

    private static async Task<IResult> AppendEventsAsync(
        [FromBody] List<AppendEventRequest> requests,
        [FromServices] EventStoreService eventStoreService,
        CancellationToken cancellationToken)
    {
        var coreRequests = requests.Select(r => new Core.Services.AppendEventRequest(
            r.EventType,
            r.AggregateId,
            r.AggregateType,
            r.Data,
            r.UserId,
            r.CorrelationId,
            r.CausationId,
            r.Version,
            r.Metadata));

        var result = await eventStoreService.AppendEventsAsync(coreRequests, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Created("/api/v1/events", result.Value.Select(ToResponse).ToList());
    }

    private static async Task<IResult> QueryEventsAsync(
        [FromQuery] Guid? aggregateId,
        [FromQuery] string? aggregateType,
        [FromQuery] string? eventType,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] Guid? correlationId,
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] EventStoreService eventStoreService,
        CancellationToken cancellationToken)
    {
        var result = await eventStoreService.GetEventsAsync(
            aggregateId,
            aggregateType,
            eventType,
            fromDate,
            toDate,
            correlationId,
            skip,
            take > 0 ? take : 100,
            cancellationToken);

        var countResult = await eventStoreService.GetEventCountAsync(
            aggregateId,
            aggregateType,
            eventType,
            fromDate,
            toDate,
            cancellationToken);

        return Results.Ok(new PagedEventsResponse
        {
            Items = result.Value.Select(ToResponse).ToList(),
            TotalCount = countResult.Value,
            Skip = skip,
            Take = take > 0 ? take : 100
        });
    }

    private static async Task<IResult> GetEventsByAggregateAsync(
        Guid aggregateId,
        [FromQuery] long? fromSequence,
        [FromQuery] long? toSequence,
        [FromServices] EventStoreService eventStoreService,
        CancellationToken cancellationToken)
    {
        var result = await eventStoreService.GetEventsByAggregateAsync(
            aggregateId,
            fromSequence,
            toSequence,
            cancellationToken);

        return Results.Ok(result.Value.Select(ToResponse).ToList());
    }

    private static async Task<IResult> ReplayEventsAsync(
        Guid aggregateId,
        [FromServices] SnapshotService snapshotService,
        CancellationToken cancellationToken)
    {
        // Simple JSON merge for replay - in production, you'd have type-specific handlers
        var result = await snapshotService.ReplayEventsAsync(
            aggregateId,
            (state, eventData) => eventData, // Simplified - just returns the latest event data
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(new ReplayResponse
        {
            AggregateId = result.Value.AggregateId,
            CurrentState = result.Value.CurrentState,
            SequenceNumber = result.Value.SequenceNumber,
            EventsApplied = result.Value.EventsApplied,
            UsedSnapshot = result.Value.UsedSnapshot
        });
    }

    private static async Task<IResult> CreateSnapshotAsync(
        Guid aggregateId,
        [FromBody] CreateSnapshotRequest request,
        [FromServices] SnapshotService snapshotService,
        CancellationToken cancellationToken)
    {
        var result = await snapshotService.CreateSnapshotAsync(
            aggregateId,
            request.AggregateType,
            request.StateData,
            request.Version,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Created($"/api/v1/events/snapshot/{result.Value.Id}", new SnapshotResponse
        {
            Id = result.Value.Id,
            AggregateId = result.Value.AggregateId,
            AggregateType = result.Value.AggregateType,
            SequenceNumber = result.Value.SequenceNumber,
            Timestamp = result.Value.Timestamp
        });
    }

    private static async Task<IResult> QueryAuditTrailAsync(
        [FromQuery] Guid? userId,
        [FromQuery] string? eventType,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] EventStoreService eventStoreService,
        CancellationToken cancellationToken)
    {
        // Audit trail is essentially the same as event query
        var result = await eventStoreService.GetEventsAsync(
            null,
            null,
            eventType,
            fromDate,
            toDate,
            null,
            skip,
            take > 0 ? take : 100,
            cancellationToken);

        var countResult = await eventStoreService.GetEventCountAsync(
            null,
            null,
            eventType,
            fromDate,
            toDate,
            cancellationToken);

        return Results.Ok(new PagedEventsResponse
        {
            Items = result.Value.Select(ToResponse).ToList(),
            TotalCount = countResult.Value,
            Skip = skip,
            Take = take > 0 ? take : 100
        });
    }

    private static StoredEventResponse ToResponse(EventStore.Core.Entities.StoredEvent e)
    {
        return new StoredEventResponse
        {
            Id = e.Id,
            EventType = e.EventType,
            AggregateId = e.AggregateId,
            AggregateType = e.AggregateType,
            SequenceNumber = e.SequenceNumber,
            Timestamp = e.Timestamp,
            UserId = e.UserId,
            CorrelationId = e.CorrelationId,
            CausationId = e.CausationId,
            Version = e.Version,
            Data = e.Data,
            Metadata = e.Metadata
        };
    }
}

// Request DTOs
public sealed record AppendEventRequest(
    string EventType,
    Guid AggregateId,
    string AggregateType,
    string Data,
    Guid? UserId = null,
    Guid? CorrelationId = null,
    Guid? CausationId = null,
    int Version = 1,
    string? Metadata = null);

public sealed record CreateSnapshotRequest(
    string AggregateType,
    string StateData,
    int Version = 1);

// Response DTOs
public sealed class StoredEventResponse
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public Guid AggregateId { get; init; }
    public string AggregateType { get; init; } = string.Empty;
    public long SequenceNumber { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid? UserId { get; init; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public int Version { get; init; }
    public string Data { get; init; } = string.Empty;
    public string? Metadata { get; init; }
}

public sealed class PagedEventsResponse
{
    public List<StoredEventResponse> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}

public sealed class ReplayResponse
{
    public Guid AggregateId { get; init; }
    public string CurrentState { get; init; } = string.Empty;
    public long SequenceNumber { get; init; }
    public int EventsApplied { get; init; }
    public bool UsedSnapshot { get; init; }
}

public sealed class SnapshotResponse
{
    public Guid Id { get; init; }
    public Guid AggregateId { get; init; }
    public string AggregateType { get; init; } = string.Empty;
    public long SequenceNumber { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
