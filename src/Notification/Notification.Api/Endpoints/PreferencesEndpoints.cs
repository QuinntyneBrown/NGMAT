using Microsoft.AspNetCore.Mvc;
using Notification.Core.Entities;
using Notification.Core.Interfaces;

namespace Notification.Api.Endpoints;

public static class PreferencesEndpoints
{
    public static void MapPreferencesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications/preferences")
            .WithTags("Notification Preferences");

        // Get preferences
        group.MapGet("/{userId}", GetPreferencesAsync)
            .WithName("GetPreferences")
            .WithDescription("Get notification preferences for a user")
            .Produces<PreferencesResponse>(StatusCodes.Status200OK);

        // Update preferences
        group.MapPut("/{userId}", UpdatePreferencesAsync)
            .WithName("UpdatePreferences")
            .WithDescription("Update notification preferences for a user")
            .Produces<PreferencesResponse>(StatusCodes.Status200OK);

        // Update email settings
        group.MapPut("/{userId}/email", UpdateEmailSettingsAsync)
            .WithName("UpdateEmailSettings")
            .WithDescription("Update email notification settings")
            .Produces(StatusCodes.Status204NoContent);

        // Update webhook settings
        group.MapPut("/{userId}/webhook", UpdateWebhookSettingsAsync)
            .WithName("UpdateWebhookSettings")
            .WithDescription("Update webhook notification settings")
            .Produces(StatusCodes.Status204NoContent);

        // Update quiet hours
        group.MapPut("/{userId}/quiet-hours", UpdateQuietHoursAsync)
            .WithName("UpdateQuietHours")
            .WithDescription("Update quiet hours settings")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetPreferencesAsync(
        [FromRoute] string userId,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var preferences = await unitOfWork.Preferences.GetByUserIdAsync(userId, cancellationToken);

        if (preferences is null)
        {
            // Create default preferences for new user
            preferences = NotificationPreferencesEntity.Create(userId);
            await unitOfWork.Preferences.AddAsync(preferences, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(ToResponse(preferences));
    }

    private static async Task<IResult> UpdatePreferencesAsync(
        [FromRoute] string userId,
        [FromBody] UpdatePreferencesRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var preferences = await unitOfWork.Preferences.GetByUserIdAsync(userId, cancellationToken);

        if (preferences is null)
        {
            preferences = NotificationPreferencesEntity.Create(userId);
            await unitOfWork.Preferences.AddAsync(preferences, cancellationToken);
        }

        if (request.EmailEnabled.HasValue || request.EmailSubscriptions is not null)
        {
            preferences.UpdateEmailSettings(
                request.EmailEnabled ?? preferences.EmailEnabled,
                request.EmailSubscriptions);
        }

        if (request.InAppEnabled.HasValue || request.InAppSubscriptions is not null)
        {
            preferences.UpdateInAppSettings(
                request.InAppEnabled ?? preferences.InAppEnabled,
                request.InAppSubscriptions);
        }

        if (request.WebhookEnabled.HasValue || request.WebhookUrl is not null)
        {
            preferences.UpdateWebhookSettings(
                request.WebhookEnabled ?? preferences.WebhookEnabled,
                request.WebhookUrl,
                request.WebhookSecret);
        }

        if (request.QuietHoursEnabled.HasValue)
        {
            preferences.UpdateQuietHours(
                request.QuietHoursEnabled.Value,
                request.QuietHoursStart,
                request.QuietHoursEnd);
        }

        if (request.DigestEnabled.HasValue)
        {
            preferences.UpdateDigestSettings(
                request.DigestEnabled.Value,
                request.DigestFrequency);
        }

        await unitOfWork.Preferences.UpdateAsync(preferences, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(preferences));
    }

    private static async Task<IResult> UpdateEmailSettingsAsync(
        [FromRoute] string userId,
        [FromBody] UpdateEmailSettingsRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var preferences = await unitOfWork.Preferences.GetByUserIdAsync(userId, cancellationToken);

        if (preferences is null)
        {
            preferences = NotificationPreferencesEntity.Create(userId);
            await unitOfWork.Preferences.AddAsync(preferences, cancellationToken);
        }

        preferences.UpdateEmailSettings(request.Enabled, request.Subscriptions);
        await unitOfWork.Preferences.UpdateAsync(preferences, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateWebhookSettingsAsync(
        [FromRoute] string userId,
        [FromBody] UpdateWebhookSettingsRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var preferences = await unitOfWork.Preferences.GetByUserIdAsync(userId, cancellationToken);

        if (preferences is null)
        {
            preferences = NotificationPreferencesEntity.Create(userId);
            await unitOfWork.Preferences.AddAsync(preferences, cancellationToken);
        }

        preferences.UpdateWebhookSettings(request.Enabled, request.WebhookUrl, request.Secret);
        await unitOfWork.Preferences.UpdateAsync(preferences, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateQuietHoursAsync(
        [FromRoute] string userId,
        [FromBody] UpdateQuietHoursRequest request,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var preferences = await unitOfWork.Preferences.GetByUserIdAsync(userId, cancellationToken);

        if (preferences is null)
        {
            preferences = NotificationPreferencesEntity.Create(userId);
            await unitOfWork.Preferences.AddAsync(preferences, cancellationToken);
        }

        preferences.UpdateQuietHours(request.Enabled, request.Start, request.End);
        await unitOfWork.Preferences.UpdateAsync(preferences, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static PreferencesResponse ToResponse(NotificationPreferencesEntity preferences) => new()
    {
        UserId = preferences.UserId,
        EmailEnabled = preferences.EmailEnabled,
        InAppEnabled = preferences.InAppEnabled,
        WebhookEnabled = preferences.WebhookEnabled,
        WebhookUrl = preferences.WebhookUrl,
        EmailSubscriptions = preferences.GetEmailSubscriptionsList(),
        InAppSubscriptions = preferences.GetInAppSubscriptionsList(),
        QuietHoursEnabled = preferences.QuietHoursEnabled,
        QuietHoursStart = preferences.QuietHoursStart,
        QuietHoursEnd = preferences.QuietHoursEnd,
        DigestEnabled = preferences.DigestEnabled,
        DigestFrequency = preferences.DigestFrequency,
        UpdatedAt = preferences.UpdatedAt
    };
}

public sealed record PreferencesResponse
{
    public string UserId { get; init; } = string.Empty;
    public bool EmailEnabled { get; init; }
    public bool InAppEnabled { get; init; }
    public bool WebhookEnabled { get; init; }
    public string? WebhookUrl { get; init; }
    public List<string> EmailSubscriptions { get; init; } = new();
    public List<string> InAppSubscriptions { get; init; } = new();
    public bool QuietHoursEnabled { get; init; }
    public TimeOnly QuietHoursStart { get; init; }
    public TimeOnly QuietHoursEnd { get; init; }
    public bool DigestEnabled { get; init; }
    public string DigestFrequency { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record UpdatePreferencesRequest
{
    public bool? EmailEnabled { get; init; }
    public bool? InAppEnabled { get; init; }
    public bool? WebhookEnabled { get; init; }
    public string? WebhookUrl { get; init; }
    public string? WebhookSecret { get; init; }
    public List<string>? EmailSubscriptions { get; init; }
    public List<string>? InAppSubscriptions { get; init; }
    public bool? QuietHoursEnabled { get; init; }
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public bool? DigestEnabled { get; init; }
    public string? DigestFrequency { get; init; }
}

public sealed record UpdateEmailSettingsRequest
{
    public bool Enabled { get; init; }
    public List<string>? Subscriptions { get; init; }
}

public sealed record UpdateWebhookSettingsRequest
{
    public bool Enabled { get; init; }
    public string? WebhookUrl { get; init; }
    public string? Secret { get; init; }
}

public sealed record UpdateQuietHoursRequest
{
    public bool Enabled { get; init; }
    public TimeOnly? Start { get; init; }
    public TimeOnly? End { get; init; }
}
