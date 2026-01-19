using Microsoft.AspNetCore.Mvc;
using Notification.Core.Entities;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Api.Endpoints;

public static class AlertRuleEndpoints
{
    public static void MapAlertRuleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications/alerts")
            .WithTags("Alert Rules");

        // Get all alert rules for user
        group.MapGet("/{userId}", GetAlertRulesAsync)
            .WithName("GetAlertRules")
            .WithDescription("Get all alert rules for a user")
            .Produces<IReadOnlyList<AlertRuleResponse>>(StatusCodes.Status200OK);

        // Get alert rule by ID
        group.MapGet("/details/{id:guid}", GetAlertRuleByIdAsync)
            .WithName("GetAlertRuleById")
            .WithDescription("Get a specific alert rule")
            .Produces<AlertRuleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Create alert rule
        group.MapPost("/", CreateAlertRuleAsync)
            .WithName("CreateAlertRule")
            .WithDescription("Create a new alert rule")
            .Produces<AlertRuleResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // Update alert rule
        group.MapPut("/{id:guid}", UpdateAlertRuleAsync)
            .WithName("UpdateAlertRule")
            .WithDescription("Update an existing alert rule")
            .Produces<AlertRuleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Delete alert rule
        group.MapDelete("/{id:guid}", DeleteAlertRuleAsync)
            .WithName("DeleteAlertRule")
            .WithDescription("Delete an alert rule")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Enable/Disable alert rule
        group.MapPut("/{id:guid}/enable", EnableAlertRuleAsync)
            .WithName("EnableAlertRule")
            .WithDescription("Enable an alert rule")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/disable", DisableAlertRuleAsync)
            .WithName("DisableAlertRule")
            .WithDescription("Disable an alert rule")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAlertRulesAsync(
        [FromRoute] string userId,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var rules = await unitOfWork.AlertRules.GetByUserAsync(userId, cancellationToken);
        var response = rules.Select(ToResponse).ToList();
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAlertRuleByIdAsync(
        [FromRoute] Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var rule = await unitOfWork.AlertRules.GetByIdAsync(id, cancellationToken);

        if (rule is null)
            return Results.NotFound();

        return Results.Ok(ToResponse(rule));
    }

    private static async Task<IResult> CreateAlertRuleAsync(
        [FromBody] CreateAlertRuleRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.Problem(
                detail: "Alert rule name is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Condition))
        {
            return Results.Problem(
                detail: "Condition is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var rule = AlertRule.Create(
            request.UserId,
            request.Name,
            request.Description ?? string.Empty,
            request.Condition,
            request.ConditionOperator,
            request.ConditionValue,
            request.NotificationChannel,
            request.NotificationTitle,
            request.NotificationMessage,
            request.NotificationType,
            request.EvaluationIntervalMinutes);

        await unitOfWork.AlertRules.AddAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/notifications/alerts/details/{rule.Id}", ToResponse(rule));
    }

    private static async Task<IResult> UpdateAlertRuleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateAlertRuleRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var rule = await unitOfWork.AlertRules.GetByIdAsync(id, cancellationToken);

        if (rule is null)
            return Results.NotFound();

        rule.Update(
            request.Name,
            request.Description,
            request.Condition,
            request.ConditionOperator,
            request.ConditionValue,
            request.NotificationChannel,
            request.NotificationTitle,
            request.NotificationMessage,
            request.EvaluationIntervalMinutes);

        await unitOfWork.AlertRules.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(rule));
    }

    private static async Task<IResult> DeleteAlertRuleAsync(
        [FromRoute] Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var rule = await unitOfWork.AlertRules.GetByIdAsync(id, cancellationToken);

        if (rule is null)
            return Results.NotFound();

        await unitOfWork.AlertRules.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> EnableAlertRuleAsync(
        [FromRoute] Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var rule = await unitOfWork.AlertRules.GetByIdAsync(id, cancellationToken);

        if (rule is null)
            return Results.NotFound();

        rule.Enable();
        await unitOfWork.AlertRules.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DisableAlertRuleAsync(
        [FromRoute] Guid id,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var rule = await unitOfWork.AlertRules.GetByIdAsync(id, cancellationToken);

        if (rule is null)
            return Results.NotFound();

        rule.Disable();
        await unitOfWork.AlertRules.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static AlertRuleResponse ToResponse(AlertRule rule) => new()
    {
        Id = rule.Id,
        UserId = rule.UserId,
        Name = rule.Name,
        Description = rule.Description,
        Condition = rule.Condition,
        ConditionOperator = rule.ConditionOperator,
        ConditionValue = rule.ConditionValue,
        NotificationChannel = rule.NotificationChannel,
        NotificationType = rule.NotificationType,
        NotificationTitle = rule.NotificationTitle,
        NotificationMessage = rule.NotificationMessage,
        IsEnabled = rule.IsEnabled,
        EvaluationIntervalMinutes = rule.EvaluationIntervalMinutes,
        LastEvaluatedAt = rule.LastEvaluatedAt,
        LastTriggeredAt = rule.LastTriggeredAt,
        TriggerCount = rule.TriggerCount,
        CreatedAt = rule.CreatedAt
    };
}

public sealed record AlertRuleResponse
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Condition { get; init; } = string.Empty;
    public string ConditionOperator { get; init; } = string.Empty;
    public string ConditionValue { get; init; } = string.Empty;
    public NotificationChannel NotificationChannel { get; init; }
    public NotificationType NotificationType { get; init; }
    public string NotificationTitle { get; init; } = string.Empty;
    public string NotificationMessage { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public int EvaluationIntervalMinutes { get; init; }
    public DateTimeOffset? LastEvaluatedAt { get; init; }
    public DateTimeOffset? LastTriggeredAt { get; init; }
    public int TriggerCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CreateAlertRuleRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Condition { get; init; } = string.Empty;
    public string ConditionOperator { get; init; } = string.Empty;
    public string ConditionValue { get; init; } = string.Empty;
    public NotificationChannel NotificationChannel { get; init; }
    public NotificationType NotificationType { get; init; } = NotificationType.Warning;
    public string NotificationTitle { get; init; } = string.Empty;
    public string NotificationMessage { get; init; } = string.Empty;
    public int EvaluationIntervalMinutes { get; init; } = 5;
}

public sealed record UpdateAlertRuleRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Condition { get; init; }
    public string? ConditionOperator { get; init; }
    public string? ConditionValue { get; init; }
    public NotificationChannel? NotificationChannel { get; init; }
    public string? NotificationTitle { get; init; }
    public string? NotificationMessage { get; init; }
    public int? EvaluationIntervalMinutes { get; init; }
}
