using Microsoft.AspNetCore.Mvc;
using Shared.Messaging.Abstractions;
using Spacecraft.Core.Entities;
using Spacecraft.Core.Services;

namespace Spacecraft.Api.Endpoints;

public static class SpacecraftEndpoints
{
    public static void MapSpacecraftEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/spacecraft")
            .WithTags("Spacecraft")
            .RequireAuthorization();

        // CRUD endpoints
        group.MapPost("/", CreateSpacecraft)
            .WithName("CreateSpacecraft")
            .WithSummary("Create a new spacecraft")
            .Produces<SpacecraftResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetSpacecraft)
            .WithName("GetSpacecraft")
            .WithSummary("Get spacecraft by ID")
            .Produces<SpacecraftResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/mission/{missionId:guid}", GetByMission)
            .WithName("GetSpacecraftByMission")
            .WithSummary("Get all spacecraft in a mission")
            .Produces<IReadOnlyList<SpacecraftResponse>>();

        group.MapPut("/{id:guid}", UpdateSpacecraft)
            .WithName("UpdateSpacecraft")
            .WithSummary("Update spacecraft properties")
            .Produces<SpacecraftResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteSpacecraft)
            .WithName("DeleteSpacecraft")
            .WithSummary("Delete spacecraft (soft delete)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // State endpoints
        group.MapGet("/{id:guid}/state", GetStateAtEpoch)
            .WithName("GetSpacecraftState")
            .WithSummary("Get spacecraft state at epoch")
            .Produces<SpacecraftStateResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/state", RecordState)
            .WithName("RecordSpacecraftState")
            .WithSummary("Record spacecraft state at epoch")
            .Produces<SpacecraftResponse>(StatusCodes.Status201Created);

        // Fuel management
        group.MapGet("/{id:guid}/fuel", GetFuelStatus)
            .WithName("GetFuelStatus")
            .WithSummary("Get current fuel status")
            .Produces<FuelStatusResponse>();

        group.MapPost("/{id:guid}/fuel/consume", ConsumeFuel)
            .WithName("ConsumeFuel")
            .WithSummary("Consume fuel")
            .Produces<SpacecraftResponse>();

        // Attitude
        group.MapPut("/{id:guid}/attitude", UpdateAttitude)
            .WithName("UpdateAttitude")
            .WithSummary("Update spacecraft attitude")
            .Produces<SpacecraftResponse>();

        // Hardware configuration
        group.MapPost("/{id:guid}/hardware/thruster", AddThruster)
            .WithName("AddThruster")
            .WithSummary("Add thruster to spacecraft")
            .Produces<ThrusterResponse>(StatusCodes.Status201Created);

        group.MapPost("/{id:guid}/hardware/fuel-tank", AddFuelTank)
            .WithName("AddFuelTank")
            .WithSummary("Add fuel tank to spacecraft")
            .Produces<FuelTankResponse>(StatusCodes.Status201Created);

        // Validation
        group.MapPost("/{id:guid}/validate", ValidateSpacecraft)
            .WithName("ValidateSpacecraft")
            .WithSummary("Validate spacecraft configuration")
            .Produces<ValidationResponse>();
    }

    private static async Task<IResult> CreateSpacecraft(
        [FromBody] CreateSpacecraftRequest request,
        [FromServices] SpacecraftService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid(); // TODO: Get from claims

        var result = await service.CreateSpacecraftAsync(
            request.Name,
            request.MissionId,
            request.DryMassKg,
            request.FuelMassKg,
            request.DragCoefficient,
            request.DragAreaM2,
            request.SrpAreaM2,
            request.ReflectivityCoefficient,
            request.InitialEpoch,
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.CoordinateFrameId,
            userId,
            request.Description,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Conflict" => Results.Conflict(result.Error.Message),
                "Validation" => Results.BadRequest(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }

        return Results.Created($"/v1/spacecraft/{result.Value.Id}", ToResponse(result.Value));
    }

    private static async Task<IResult> GetSpacecraft(
        Guid id,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetSpacecraftAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> GetByMission(
        Guid missionId,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByMissionAsync(missionId, cancellationToken);
        return Results.Ok(result.Value.Select(ToResponse).ToList());
    }

    private static async Task<IResult> UpdateSpacecraft(
        Guid id,
        [FromBody] UpdateSpacecraftRequest request,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid(); // TODO: Get from claims

        var result = await service.UpdateSpacecraftAsync(
            id, userId,
            request.Name,
            request.Description,
            request.DryMassKg,
            request.DragCoefficient,
            request.DragAreaM2,
            request.SrpAreaM2,
            request.ReflectivityCoefficient,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.BadRequest(result.Error.Message);
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> DeleteSpacecraft(
        Guid id,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid(); // TODO: Get from claims

        var result = await service.DeleteSpacecraftAsync(id, userId, cancellationToken);
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetStateAtEpoch(
        Guid id,
        [FromQuery] DateTime epoch,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetStateAtEpochAsync(id, epoch, cancellationToken);
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Ok(ToStateResponse(result.Value));
    }

    private static async Task<IResult> RecordState(
        Guid id,
        [FromBody] RecordStateRequest request,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RecordStateAsync(
            id, request.Epoch,
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.FuelMassKg,
            request.CoordinateFrameId,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Created($"/v1/spacecraft/{id}/state?epoch={request.Epoch:O}", ToResponse(result.Value));
    }

    private static async Task<IResult> GetFuelStatus(
        Guid id,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetSpacecraftAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        var spacecraft = result.Value;
        return Results.Ok(new FuelStatusResponse
        {
            SpacecraftId = spacecraft.Id,
            FuelMassKg = spacecraft.FuelMassKg,
            DryMassKg = spacecraft.DryMassKg,
            TotalMassKg = spacecraft.TotalMassKg,
            FuelTanks = spacecraft.FuelTanks.Select(t => new FuelTankStatusResponse
            {
                Id = t.Id,
                Name = t.Name,
                FuelType = t.FuelType.ToString(),
                CapacityKg = t.CapacityKg,
                CurrentMassKg = t.CurrentMassKg,
                FillPercentage = t.FillPercentage
            }).ToList()
        });
    }

    private static async Task<IResult> ConsumeFuel(
        Guid id,
        [FromBody] ConsumeFuelRequest request,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ConsumeFuelAsync(id, request.AmountKg, request.ManeuverId, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.BadRequest(result.Error.Message);
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> UpdateAttitude(
        Guid id,
        [FromBody] UpdateAttitudeRequest request,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAttitudeAsync(
            id, request.Mode,
            request.Q0, request.Q1, request.Q2, request.Q3,
            request.SpinRateRadPerSec,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Ok(ToResponse(result.Value));
    }

    private static async Task<IResult> AddThruster(
        Guid id,
        [FromBody] AddThrusterRequest request,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.AddThrusterAsync(
            id, request.Name, request.Type,
            request.ThrustN, request.IspSeconds, request.MassKg, request.FuelType,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Created($"/v1/spacecraft/{id}/hardware/thruster/{result.Value.Id}", ToThrusterResponse(result.Value));
    }

    private static async Task<IResult> AddFuelTank(
        Guid id,
        [FromBody] AddFuelTankRequest request,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.AddFuelTankAsync(
            id, request.Name, request.FuelType,
            request.CapacityKg, request.InitialMassKg, request.PressurePa, request.TankMassKg,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Created($"/v1/spacecraft/{id}/hardware/fuel-tank/{result.Value.Id}", ToFuelTankResponse(result.Value));
    }

    private static async Task<IResult> ValidateSpacecraft(
        Guid id,
        [FromServices] SpacecraftService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ValidateSpacecraftAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error.Message);
        }

        return Results.Ok(new ValidationResponse
        {
            IsValid = result.Value.IsValid,
            Errors = result.Value.Errors.ToList(),
            Warnings = result.Value.Warnings.ToList()
        });
    }

    // Response mappers
    private static SpacecraftResponse ToResponse(SpacecraftEntity spacecraft) => new()
    {
        Id = spacecraft.Id,
        Name = spacecraft.Name,
        Description = spacecraft.Description,
        MissionId = spacecraft.MissionId,
        DryMassKg = spacecraft.DryMassKg,
        FuelMassKg = spacecraft.FuelMassKg,
        TotalMassKg = spacecraft.TotalMassKg,
        DragCoefficient = spacecraft.DragCoefficient,
        DragAreaM2 = spacecraft.DragAreaM2,
        SrpAreaM2 = spacecraft.SrpAreaM2,
        ReflectivityCoefficient = spacecraft.ReflectivityCoefficient,
        InitialEpoch = spacecraft.InitialEpoch,
        InitialPosition = new[] { spacecraft.InitialX, spacecraft.InitialY, spacecraft.InitialZ },
        InitialVelocity = new[] { spacecraft.InitialVx, spacecraft.InitialVy, spacecraft.InitialVz },
        CoordinateFrameId = spacecraft.CoordinateFrameId,
        AttitudeMode = spacecraft.AttitudeMode.ToString(),
        Quaternion = new[] { spacecraft.AttitudeQ0, spacecraft.AttitudeQ1, spacecraft.AttitudeQ2, spacecraft.AttitudeQ3 },
        SpinRateRadPerSec = spacecraft.SpinRateRadPerSec,
        ThrusterCount = spacecraft.Thrusters.Count,
        FuelTankCount = spacecraft.FuelTanks.Count,
        CreatedAt = spacecraft.CreatedAt,
        UpdatedAt = spacecraft.UpdatedAt
    };

    private static SpacecraftStateResponse ToStateResponse(SpacecraftState state) => new()
    {
        Id = state.Id,
        SpacecraftId = state.SpacecraftId,
        Epoch = state.Epoch,
        Position = new[] { state.X, state.Y, state.Z },
        Velocity = new[] { state.Vx, state.Vy, state.Vz },
        PositionMagnitudeKm = state.PositionMagnitude,
        VelocityMagnitudeKmPerSec = state.VelocityMagnitude,
        AltitudeKm = state.Altitude,
        FuelMassKg = state.FuelMassKg,
        CoordinateFrameId = state.CoordinateFrameId,
        RecordedAt = state.RecordedAt
    };

    private static ThrusterResponse ToThrusterResponse(Thruster thruster) => new()
    {
        Id = thruster.Id,
        Name = thruster.Name,
        Type = thruster.Type.ToString(),
        ThrustN = thruster.ThrustN,
        IspSeconds = thruster.IspSeconds,
        MassKg = thruster.MassKg,
        FuelType = thruster.FuelType.ToString(),
        ExhaustVelocityKmPerSec = thruster.ExhaustVelocityKmPerSec,
        MassFlowRateKgPerSec = thruster.MassFlowRateKgPerSec,
        IsOperational = thruster.IsOperational
    };

    private static FuelTankResponse ToFuelTankResponse(FuelTank tank) => new()
    {
        Id = tank.Id,
        Name = tank.Name,
        FuelType = tank.FuelType.ToString(),
        CapacityKg = tank.CapacityKg,
        CurrentMassKg = tank.CurrentMassKg,
        PressurePa = tank.PressurePa,
        TankMassKg = tank.MassKg,
        FillPercentage = tank.FillPercentage
    };
}

// Request/Response DTOs
public record CreateSpacecraftRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid MissionId { get; init; }
    public double DryMassKg { get; init; }
    public double FuelMassKg { get; init; }
    public double DragCoefficient { get; init; }
    public double DragAreaM2 { get; init; }
    public double SrpAreaM2 { get; init; }
    public double ReflectivityCoefficient { get; init; }
    public DateTime InitialEpoch { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
    public Guid CoordinateFrameId { get; init; }
}

public record UpdateSpacecraftRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public double? DryMassKg { get; init; }
    public double? DragCoefficient { get; init; }
    public double? DragAreaM2 { get; init; }
    public double? SrpAreaM2 { get; init; }
    public double? ReflectivityCoefficient { get; init; }
}

public record RecordStateRequest
{
    public DateTime Epoch { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
    public double FuelMassKg { get; init; }
    public Guid CoordinateFrameId { get; init; }
}

public record ConsumeFuelRequest
{
    public double AmountKg { get; init; }
    public Guid? ManeuverId { get; init; }
}

public record UpdateAttitudeRequest
{
    public AttitudeMode Mode { get; init; }
    public double Q0 { get; init; } = 1;
    public double Q1 { get; init; }
    public double Q2 { get; init; }
    public double Q3 { get; init; }
    public double SpinRateRadPerSec { get; init; }
}

public record AddThrusterRequest
{
    public required string Name { get; init; }
    public ThrusterType Type { get; init; }
    public double ThrustN { get; init; }
    public double IspSeconds { get; init; }
    public double MassKg { get; init; }
    public FuelType FuelType { get; init; }
}

public record AddFuelTankRequest
{
    public required string Name { get; init; }
    public FuelType FuelType { get; init; }
    public double CapacityKg { get; init; }
    public double InitialMassKg { get; init; }
    public double PressurePa { get; init; }
    public double TankMassKg { get; init; }
}

public record SpacecraftResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid MissionId { get; init; }
    public double DryMassKg { get; init; }
    public double FuelMassKg { get; init; }
    public double TotalMassKg { get; init; }
    public double DragCoefficient { get; init; }
    public double DragAreaM2 { get; init; }
    public double SrpAreaM2 { get; init; }
    public double ReflectivityCoefficient { get; init; }
    public DateTime InitialEpoch { get; init; }
    public double[] InitialPosition { get; init; } = Array.Empty<double>();
    public double[] InitialVelocity { get; init; } = Array.Empty<double>();
    public Guid CoordinateFrameId { get; init; }
    public string AttitudeMode { get; init; } = string.Empty;
    public double[] Quaternion { get; init; } = Array.Empty<double>();
    public double SpinRateRadPerSec { get; init; }
    public int ThrusterCount { get; init; }
    public int FuelTankCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record SpacecraftStateResponse
{
    public Guid Id { get; init; }
    public Guid SpacecraftId { get; init; }
    public DateTime Epoch { get; init; }
    public double[] Position { get; init; } = Array.Empty<double>();
    public double[] Velocity { get; init; } = Array.Empty<double>();
    public double PositionMagnitudeKm { get; init; }
    public double VelocityMagnitudeKmPerSec { get; init; }
    public double AltitudeKm { get; init; }
    public double FuelMassKg { get; init; }
    public Guid CoordinateFrameId { get; init; }
    public DateTime RecordedAt { get; init; }
}

public record FuelStatusResponse
{
    public Guid SpacecraftId { get; init; }
    public double FuelMassKg { get; init; }
    public double DryMassKg { get; init; }
    public double TotalMassKg { get; init; }
    public IReadOnlyList<FuelTankStatusResponse> FuelTanks { get; init; } = Array.Empty<FuelTankStatusResponse>();
}

public record FuelTankStatusResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FuelType { get; init; } = string.Empty;
    public double CapacityKg { get; init; }
    public double CurrentMassKg { get; init; }
    public double FillPercentage { get; init; }
}

public record ThrusterResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public double ThrustN { get; init; }
    public double IspSeconds { get; init; }
    public double MassKg { get; init; }
    public string FuelType { get; init; } = string.Empty;
    public double ExhaustVelocityKmPerSec { get; init; }
    public double MassFlowRateKgPerSec { get; init; }
    public bool IsOperational { get; init; }
}

public record FuelTankResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FuelType { get; init; } = string.Empty;
    public double CapacityKg { get; init; }
    public double CurrentMassKg { get; init; }
    public double PressurePa { get; init; }
    public double TankMassKg { get; init; }
    public double FillPercentage { get; init; }
}

public record ValidationResponse
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
