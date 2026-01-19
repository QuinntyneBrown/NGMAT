using Microsoft.AspNetCore.Mvc;
using Maneuver.Core.Entities;
using Maneuver.Core.Services;

namespace Maneuver.Api.Endpoints;

public static class ManeuverEndpoints
{
    public static IEndpointRouteBuilder MapManeuverEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/maneuvers")
            .WithTags("Maneuvers")
            .RequireAuthorization();

        // Maneuver Plan CRUD
        group.MapGet("/", GetAllManeuvers)
            .WithName("GetAllManeuvers")
            .WithSummary("Get all maneuver plans");

        group.MapGet("/{id:guid}", GetManeuver)
            .WithName("GetManeuver")
            .WithSummary("Get a maneuver plan by ID");

        group.MapGet("/spacecraft/{spacecraftId:guid}", GetManeuversBySpacecraft)
            .WithName("GetManeuversBySpacecraft")
            .WithSummary("Get all maneuvers for a spacecraft");

        group.MapGet("/mission/{missionId:guid}", GetManeuversByMission)
            .WithName("GetManeuversByMission")
            .WithSummary("Get all maneuvers for a mission");

        // Create Maneuvers
        group.MapPost("/impulsive", CreateImpulsiveManeuver)
            .WithName("CreateImpulsiveManeuver")
            .WithSummary("Create an impulsive (instantaneous) maneuver");

        group.MapPost("/finite", CreateFiniteManeuver)
            .WithName("CreateFiniteManeuver")
            .WithSummary("Create a finite burn maneuver");

        // Maneuver Lifecycle
        group.MapPost("/{id:guid}/schedule", ScheduleManeuver)
            .WithName("ScheduleManeuver")
            .WithSummary("Schedule a planned maneuver");

        group.MapPost("/{id:guid}/cancel", CancelManeuver)
            .WithName("CancelManeuver")
            .WithSummary("Cancel a planned or scheduled maneuver");

        group.MapDelete("/{id:guid}", DeleteManeuver)
            .WithName("DeleteManeuver")
            .WithSummary("Delete a maneuver plan");

        // Transfer Orbit Calculations
        group.MapPost("/calculate/hohmann", CalculateHohmann)
            .WithName("CalculateHohmann")
            .WithSummary("Calculate a Hohmann transfer orbit");

        group.MapPost("/calculate/bi-elliptic", CalculateBiElliptic)
            .WithName("CalculateBiElliptic")
            .WithSummary("Calculate a bi-elliptic transfer orbit");

        group.MapPost("/calculate/plane-change", CalculatePlaneChange)
            .WithName("CalculatePlaneChange")
            .WithSummary("Calculate a plane change maneuver");

        group.MapPost("/calculate/compare-transfers", CompareTransfers)
            .WithName("CompareTransfers")
            .WithSummary("Compare Hohmann and bi-elliptic transfers");

        group.MapPost("/calculate/fuel-mass", CalculateFuelMass)
            .WithName("CalculateFuelMass")
            .WithSummary("Calculate required fuel mass for a delta-V");

        return app;
    }

    // Handlers

    private static async Task<IResult> GetAllManeuvers(
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllManeuversAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(ManeuverPlanResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetManeuver(
        Guid id,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetManeuverAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(ManeuverPlanResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetManeuversBySpacecraft(
        Guid spacecraftId,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetManeuversBySpacecraftAsync(spacecraftId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(ManeuverPlanResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetManeuversByMission(
        Guid missionId,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetManeuversByMissionAsync(missionId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(ManeuverPlanResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> CreateImpulsiveManeuver(
        [FromBody] CreateImpulsiveManeuverRequest request,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateImpulsiveManeuverAsync(
            request.Name,
            request.SpacecraftId,
            request.PlannedEpoch,
            request.DeltaVx,
            request.DeltaVy,
            request.DeltaVz,
            request.CoordinateFrame,
            request.SpacecraftMassKg,
            request.SpecificImpulseS,
            request.CreatedByUserId,
            request.Description,
            request.MissionId,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }
        return Results.Created($"/api/maneuvers/{result.Value!.Id}", ManeuverPlanResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> CreateFiniteManeuver(
        [FromBody] CreateFiniteManeuverRequest request,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateFiniteManeuverAsync(
            request.Name,
            request.SpacecraftId,
            request.PlannedEpoch,
            request.ThrustMagnitudeN,
            request.BurnDurationSeconds,
            request.ThrustDirectionX,
            request.ThrustDirectionY,
            request.ThrustDirectionZ,
            request.CoordinateFrame,
            request.SpacecraftMassKg,
            request.SpecificImpulseS,
            request.CreatedByUserId,
            request.Description,
            request.MissionId,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }
        return Results.Created($"/api/maneuvers/{result.Value!.Id}", ManeuverPlanResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> ScheduleManeuver(
        Guid id,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ScheduleManeuverAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { Message = "Maneuver scheduled successfully" })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> CancelManeuver(
        Guid id,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CancelManeuverAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { Message = "Maneuver cancelled successfully" })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> DeleteManeuver(
        Guid id,
        [FromServices] ManeuverService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteManeuverAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error.Message);
    }

    private static IResult CalculateHohmann(
        [FromBody] HohmannTransferRequest request,
        [FromServices] ManeuverService service)
    {
        var result = service.CalculateHohmannTransfer(
            request.InitialRadiusM,
            request.FinalRadiusM,
            request.CentralBodyGM ?? 3.986004418e14);

        return result.IsSuccess
            ? Results.Ok(TransferOrbitResponse.FromResult(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CalculateBiElliptic(
        [FromBody] BiEllipticTransferRequest request,
        [FromServices] ManeuverService service)
    {
        var result = service.CalculateBiEllipticTransfer(
            request.InitialRadiusM,
            request.FinalRadiusM,
            request.IntermediateRadiusM,
            request.CentralBodyGM ?? 3.986004418e14);

        return result.IsSuccess
            ? Results.Ok(TransferOrbitResponse.FromResult(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CalculatePlaneChange(
        [FromBody] PlaneChangeRequest request,
        [FromServices] ManeuverService service)
    {
        var result = service.CalculatePlaneChange(
            request.OrbitRadiusM,
            request.InclinationChangeDeg,
            request.BurnAtAscendingNode,
            request.CentralBodyGM ?? 3.986004418e14);

        return result.IsSuccess
            ? Results.Ok(TransferOrbitResponse.FromResult(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CompareTransfers(
        [FromBody] CompareTransfersRequest request,
        [FromServices] ManeuverService service)
    {
        var result = service.CompareTransfers(
            request.InitialRadiusM,
            request.FinalRadiusM,
            request.CentralBodyGM ?? 3.986004418e14);

        return result.IsSuccess
            ? Results.Ok(TransferOrbitResponse.FromResult(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CalculateFuelMass(
        [FromBody] FuelMassRequest request,
        [FromServices] ManeuverService service)
    {
        var result = service.CalculateFuelMass(
            request.InitialMassKg,
            request.DeltaVMps,
            request.SpecificImpulseS);

        return result.IsSuccess
            ? Results.Ok(new FuelMassResponse(
                request.InitialMassKg,
                request.DeltaVMps,
                request.SpecificImpulseS,
                result.Value!,
                request.InitialMassKg - result.Value!))
            : Results.Problem(result.Error.Message);
    }
}

// Request DTOs
public record CreateImpulsiveManeuverRequest(
    string Name,
    Guid SpacecraftId,
    DateTime PlannedEpoch,
    double DeltaVx,
    double DeltaVy,
    double DeltaVz,
    CoordinateFrame CoordinateFrame,
    double SpacecraftMassKg,
    double SpecificImpulseS,
    string CreatedByUserId,
    string? Description = null,
    Guid? MissionId = null);

public record CreateFiniteManeuverRequest(
    string Name,
    Guid SpacecraftId,
    DateTime PlannedEpoch,
    double ThrustMagnitudeN,
    double BurnDurationSeconds,
    double ThrustDirectionX,
    double ThrustDirectionY,
    double ThrustDirectionZ,
    CoordinateFrame CoordinateFrame,
    double SpacecraftMassKg,
    double SpecificImpulseS,
    string CreatedByUserId,
    string? Description = null,
    Guid? MissionId = null);

public record HohmannTransferRequest(
    double InitialRadiusM,
    double FinalRadiusM,
    double? CentralBodyGM = null);

public record BiEllipticTransferRequest(
    double InitialRadiusM,
    double FinalRadiusM,
    double IntermediateRadiusM,
    double? CentralBodyGM = null);

public record PlaneChangeRequest(
    double OrbitRadiusM,
    double InclinationChangeDeg,
    bool BurnAtAscendingNode = true,
    double? CentralBodyGM = null);

public record CompareTransfersRequest(
    double InitialRadiusM,
    double FinalRadiusM,
    double? CentralBodyGM = null);

public record FuelMassRequest(
    double InitialMassKg,
    double DeltaVMps,
    double SpecificImpulseS);

// Response DTOs
public record ManeuverPlanResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid SpacecraftId,
    Guid? MissionId,
    string Type,
    string Status,
    DateTime PlannedEpoch,
    DateTime? ExecutedEpoch,
    double DeltaVx,
    double DeltaVy,
    double DeltaVz,
    double DeltaVMagnitude,
    string CoordinateFrame,
    double? ThrustMagnitudeN,
    double? BurnDurationSeconds,
    double? SpecificImpulseS,
    double EstimatedFuelMassKg,
    double? ActualFuelMassKg,
    double SpacecraftMassBeforeKg,
    double SpacecraftMassAfterKg,
    DateTime CreatedAt)
{
    public static ManeuverPlanResponse FromEntity(ManeuverPlan entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.SpacecraftId,
        entity.MissionId,
        entity.Type.ToString(),
        entity.Status.ToString(),
        entity.PlannedEpoch,
        entity.ExecutedEpoch,
        entity.DeltaVx,
        entity.DeltaVy,
        entity.DeltaVz,
        entity.DeltaVMagnitude,
        entity.CoordinateFrame.ToString(),
        entity.ThrustMagnitudeN,
        entity.BurnDurationSeconds,
        entity.SpecificImpulseS,
        entity.EstimatedFuelMassKg,
        entity.ActualFuelMassKg,
        entity.SpacecraftMassBeforeKg,
        entity.SpacecraftMassAfterKg,
        entity.CreatedAt);
}

public record TransferBurnResponse(
    int BurnNumber,
    string Location,
    double DeltaVMps,
    double TimeFromStartSeconds);

public record TransferOrbitResponse(
    Guid Id,
    string TransferType,
    double InitialRadiusM,
    double FinalRadiusM,
    double? IntermediateRadiusM,
    IReadOnlyList<TransferBurnResponse> Burns,
    double TotalDeltaVMps,
    double TransferTimeSeconds,
    double TransferTimeHours,
    DateTime CalculatedAt)
{
    public static TransferOrbitResponse FromResult(TransferOrbitResult result) => new(
        result.Id,
        result.TransferType,
        result.InitialRadiusM,
        result.FinalRadiusM,
        result.IntermediateRadiusM,
        result.Burns.Select(b => new TransferBurnResponse(b.BurnNumber, b.Location, b.DeltaVMps, b.TimeFromStartSeconds)).ToList(),
        result.TotalDeltaVMps,
        result.TransferTimeSeconds,
        result.TransferTimeSeconds / 3600.0,
        result.CalculatedAt);
}

public record FuelMassResponse(
    double InitialMassKg,
    double DeltaVMps,
    double SpecificImpulseS,
    double FuelMassKg,
    double FinalMassKg);
