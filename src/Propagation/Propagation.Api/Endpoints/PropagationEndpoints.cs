using Microsoft.AspNetCore.Mvc;
using Propagation.Core.Entities;
using Propagation.Core.Services;

namespace Propagation.Api.Endpoints;

public static class PropagationEndpoints
{
    public static IEndpointRouteBuilder MapPropagationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/propagation")
            .WithTags("Propagation")
            .RequireAuthorization();

        // Configuration endpoints
        group.MapGet("/configurations", GetAllConfigurations)
            .WithName("GetAllPropagationConfigurations")
            .WithSummary("Get all propagation configurations");

        group.MapGet("/configurations/{id:guid}", GetConfiguration)
            .WithName("GetPropagationConfiguration")
            .WithSummary("Get a propagation configuration by ID");

        group.MapPost("/configurations", CreateConfiguration)
            .WithName("CreatePropagationConfiguration")
            .WithSummary("Create a new propagation configuration");

        group.MapPost("/configurations/import-standard", CreateStandardConfigurations)
            .WithName("CreateStandardPropagationConfigurations")
            .WithSummary("Create standard propagation configurations (Fast, Precise, Long Term)");

        group.MapPut("/configurations/{id:guid}/integrator", UpdateIntegratorSettings)
            .WithName("UpdateIntegratorSettings")
            .WithSummary("Update integrator settings for a configuration");

        group.MapPut("/configurations/{id:guid}/output", UpdateOutputSettings)
            .WithName("UpdateOutputSettings")
            .WithSummary("Update output settings for a configuration");

        group.MapPut("/configurations/{id:guid}/stopping-conditions", UpdateStoppingConditions)
            .WithName("UpdateStoppingConditions")
            .WithSummary("Update stopping conditions for a configuration");

        group.MapPut("/configurations/{id:guid}/force-model", SetForceModelReference)
            .WithName("SetForceModelReference")
            .WithSummary("Set the force model configuration reference");

        group.MapDelete("/configurations/{id:guid}", DeleteConfiguration)
            .WithName("DeletePropagationConfiguration")
            .WithSummary("Delete a propagation configuration");

        // Propagation execution endpoints
        group.MapPost("/propagate", Propagate)
            .WithName("Propagate")
            .WithSummary("Execute orbit propagation");

        group.MapPost("/propagate/two-body", PropagateTwoBody)
            .WithName("PropagateTwoBody")
            .WithSummary("Execute simple two-body orbit propagation");

        // Result endpoints
        group.MapGet("/results/{id:guid}", GetResult)
            .WithName("GetPropagationResult")
            .WithSummary("Get a propagation result by ID");

        group.MapGet("/results/recent", GetRecentResults)
            .WithName("GetRecentPropagationResults")
            .WithSummary("Get recent propagation results");

        return app;
    }

    // Configuration Handlers
    private static async Task<IResult> GetAllConfigurations(
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllConfigurationsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(PropagationConfigurationResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetConfiguration(
        Guid id,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetConfigurationAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(PropagationConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> CreateConfiguration(
        [FromBody] CreateConfigurationRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateConfigurationAsync(
            request.Name, request.CreatedByUserId, request.Description, request.MissionId, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("Conflict")
                ? Results.Conflict(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Created($"/api/propagation/configurations/{result.Value!.Id}", PropagationConfigurationResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> CreateStandardConfigurations(
        [FromQuery] string userId,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateStandardConfigurationsAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { CreatedCount = result.Value })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> UpdateIntegratorSettings(
        Guid id,
        [FromBody] UpdateIntegratorSettingsRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateIntegratorSettingsAsync(
            id,
            request.Integrator,
            request.InitialStepSizeSeconds,
            request.MinStepSizeSeconds,
            request.MaxStepSizeSeconds,
            request.RelativeTolerance,
            request.AbsoluteTolerance,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(PropagationConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> UpdateOutputSettings(
        Guid id,
        [FromBody] UpdateOutputSettingsRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateOutputSettingsAsync(
            id, request.OutputMode, request.OutputStepSizeSeconds, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(PropagationConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> UpdateStoppingConditions(
        Guid id,
        [FromBody] UpdateStoppingConditionsRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var configResult = await service.GetConfigurationAsync(id, cancellationToken);
        if (configResult.IsFailure)
        {
            return configResult.Error.Code.Contains("NotFound")
                ? Results.NotFound(configResult.Error.Message)
                : Results.Problem(configResult.Error.Message);
        }

        configResult.Value!.SetStoppingConditions(
            request.MaxPropagationDurationSeconds,
            request.MaxStepCount,
            request.MinAltitudeMeters);

        return Results.Ok(PropagationConfigurationResponse.FromEntity(configResult.Value));
    }

    private static async Task<IResult> SetForceModelReference(
        Guid id,
        [FromBody] SetForceModelReferenceRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var configResult = await service.GetConfigurationAsync(id, cancellationToken);
        if (configResult.IsFailure)
        {
            return configResult.Error.Code.Contains("NotFound")
                ? Results.NotFound(configResult.Error.Message)
                : Results.Problem(configResult.Error.Message);
        }

        configResult.Value!.SetForceModelReference(request.ForceModelConfigurationId);
        return Results.Ok(PropagationConfigurationResponse.FromEntity(configResult.Value));
    }

    private static async Task<IResult> DeleteConfiguration(
        Guid id,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteConfigurationAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error.Message);
    }

    // Propagation Execution Handlers
    private static async Task<IResult> Propagate(
        [FromBody] PropagateRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var initialState = new PropagationState(
            request.InitialEpoch,
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz);

        PropagationConfiguration? config = null;
        if (request.ConfigurationId.HasValue)
        {
            var configResult = await service.GetConfigurationAsync(request.ConfigurationId.Value, cancellationToken);
            if (configResult.IsSuccess)
            {
                config = configResult.Value;
            }
        }

        // Use two-body acceleration if no custom acceleration provider
        AccelerationProvider accelerationProvider = TwoBodyAccelerationProvider(request.CentralBodyGM ?? 3.986004418e14);

        var result = await service.PropagateAsync(
            initialState, request.EndEpoch, accelerationProvider, config, request.SpacecraftId, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(PropagationResultResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> PropagateTwoBody(
        [FromBody] PropagateTwoBodyRequest request,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var initialState = new PropagationState(
            request.InitialEpoch,
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz);

        var accelerationProvider = TwoBodyAccelerationProvider(request.CentralBodyGM);

        var result = await service.PropagateAsync(
            initialState, request.EndEpoch, accelerationProvider, null, request.SpacecraftId, cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(PropagationResultResponse.FromEntity(result.Value!));
    }

    private static AccelerationProvider TwoBodyAccelerationProvider(double gm)
    {
        return (epoch, state) =>
        {
            var r = state.Radius;
            var r3 = r * r * r;
            var factor = -gm / r3;

            return new StateDerivative(
                state.Vx, state.Vy, state.Vz,
                factor * state.X,
                factor * state.Y,
                factor * state.Z);
        };
    }

    // Result Handlers
    private static async Task<IResult> GetResult(
        Guid id,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetResultAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(PropagationResultResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetRecentResults(
        [FromQuery] int count,
        [FromServices] PropagationService service,
        CancellationToken cancellationToken)
    {
        var effectiveCount = count > 0 ? count : 10;
        var result = await service.GetRecentResultsAsync(effectiveCount, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(PropagationResultSummaryResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }
}

// Request DTOs
public record CreateConfigurationRequest(
    string Name,
    string CreatedByUserId,
    string? Description = null,
    Guid? MissionId = null);

public record UpdateIntegratorSettingsRequest(
    IntegratorType Integrator,
    double InitialStepSizeSeconds,
    double MinStepSizeSeconds,
    double MaxStepSizeSeconds,
    double RelativeTolerance,
    double AbsoluteTolerance);

public record UpdateOutputSettingsRequest(
    OutputMode OutputMode,
    double OutputStepSizeSeconds);

public record UpdateStoppingConditionsRequest(
    double? MaxPropagationDurationSeconds,
    double? MaxStepCount,
    double? MinAltitudeMeters);

public record SetForceModelReferenceRequest(
    Guid? ForceModelConfigurationId);

public record PropagateRequest(
    DateTime InitialEpoch,
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    DateTime EndEpoch,
    Guid? ConfigurationId = null,
    Guid? SpacecraftId = null,
    double? CentralBodyGM = null);

public record PropagateTwoBodyRequest(
    DateTime InitialEpoch,
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    DateTime EndEpoch,
    double CentralBodyGM = 3.986004418e14,
    Guid? SpacecraftId = null);

// Response DTOs
public record PropagationConfigurationResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? MissionId,
    string Integrator,
    double InitialStepSizeSeconds,
    double MinStepSizeSeconds,
    double MaxStepSizeSeconds,
    double RelativeTolerance,
    double AbsoluteTolerance,
    string OutputMode,
    double OutputStepSizeSeconds,
    Guid? ForceModelConfigurationId,
    double? MaxPropagationDurationSeconds,
    double? MaxStepCount,
    double? MinAltitudeMeters,
    DateTime CreatedAt)
{
    public static PropagationConfigurationResponse FromEntity(PropagationConfiguration entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.MissionId,
        entity.Integrator.ToString(),
        entity.InitialStepSizeSeconds,
        entity.MinStepSizeSeconds,
        entity.MaxStepSizeSeconds,
        entity.RelativeTolerance,
        entity.AbsoluteTolerance,
        entity.OutputMode.ToString(),
        entity.OutputStepSizeSeconds,
        entity.ForceModelConfigurationId,
        entity.MaxPropagationDurationSeconds,
        entity.MaxStepCount,
        entity.MinAltitudeMeters,
        entity.CreatedAt);
}

public record PropagationStateResponse(
    DateTime Epoch,
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    double Radius,
    double Speed,
    double Altitude)
{
    public static PropagationStateResponse FromEntity(PropagationState state) => new(
        state.Epoch,
        state.X, state.Y, state.Z,
        state.Vx, state.Vy, state.Vz,
        state.Radius,
        state.Speed,
        state.Altitude);
}

public record PropagationResultResponse(
    Guid Id,
    Guid? PropagationConfigurationId,
    Guid? SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    IReadOnlyList<PropagationStateResponse> States,
    int StateCount,
    int StepCount,
    double ComputationTimeMs,
    bool WasSuccessful,
    string? ErrorMessage,
    string TerminationReason,
    DateTime CreatedAt)
{
    public static PropagationResultResponse FromEntity(PropagationResult entity) => new(
        entity.Id,
        entity.PropagationConfigurationId,
        entity.SpacecraftId,
        entity.StartEpoch,
        entity.EndEpoch,
        entity.States.Select(PropagationStateResponse.FromEntity).ToList(),
        entity.States.Count,
        entity.StepCount,
        entity.ComputationTimeMs,
        entity.WasSuccessful,
        entity.ErrorMessage,
        entity.TerminationReason.ToString(),
        entity.CreatedAt);
}

public record PropagationResultSummaryResponse(
    Guid Id,
    Guid? SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    int StateCount,
    int StepCount,
    double ComputationTimeMs,
    bool WasSuccessful,
    string TerminationReason,
    DateTime CreatedAt)
{
    public static PropagationResultSummaryResponse FromEntity(PropagationResult entity) => new(
        entity.Id,
        entity.SpacecraftId,
        entity.StartEpoch,
        entity.EndEpoch,
        entity.States.Count,
        entity.StepCount,
        entity.ComputationTimeMs,
        entity.WasSuccessful,
        entity.TerminationReason.ToString(),
        entity.CreatedAt);
}
