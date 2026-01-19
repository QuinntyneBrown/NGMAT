using Microsoft.AspNetCore.Mvc;
using ForceModel.Core.Entities;
using ForceModel.Core.Services;

namespace ForceModel.Api.Endpoints;

public static class ForceModelEndpoints
{
    public static IEndpointRouteBuilder MapForceModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/forcemodel")
            .WithTags("Force Model")
            .RequireAuthorization();

        // Configuration endpoints
        group.MapGet("/configurations", GetAllConfigurations)
            .WithName("GetAllForceModelConfigurations")
            .WithSummary("Get all force model configurations");

        group.MapGet("/configurations/{id:guid}", GetConfiguration)
            .WithName("GetForceModelConfiguration")
            .WithSummary("Get a force model configuration by ID");

        group.MapPost("/configurations", CreateConfiguration)
            .WithName("CreateForceModelConfiguration")
            .WithSummary("Create a new force model configuration");

        group.MapPost("/configurations/import-standard", CreateStandardConfigurations)
            .WithName("CreateStandardForceModelConfigurations")
            .WithSummary("Create standard force model configurations (Low, Medium, High Fidelity)");

        group.MapPut("/configurations/{id:guid}/gravity", UpdateGravitySettings)
            .WithName("UpdateGravitySettings")
            .WithSummary("Update gravity settings for a configuration");

        group.MapPut("/configurations/{id:guid}/atmosphere", UpdateAtmosphereSettings)
            .WithName("UpdateAtmosphereSettings")
            .WithSummary("Update atmosphere settings for a configuration");

        group.MapPut("/configurations/{id:guid}/srp", UpdateSrpSettings)
            .WithName("UpdateSrpSettings")
            .WithSummary("Update solar radiation pressure settings for a configuration");

        group.MapPut("/configurations/{id:guid}/third-body", UpdateThirdBodySettings)
            .WithName("UpdateThirdBodySettings")
            .WithSummary("Update third body settings for a configuration");

        group.MapDelete("/configurations/{id:guid}", DeleteConfiguration)
            .WithName("DeleteForceModelConfiguration")
            .WithSummary("Delete a force model configuration");

        // Calculation endpoints
        group.MapPost("/calculate", CalculateAcceleration)
            .WithName("CalculateAcceleration")
            .WithSummary("Calculate total acceleration for a spacecraft state");

        group.MapPost("/calculate/gravity", CalculateGravity)
            .WithName("CalculateGravity")
            .WithSummary("Calculate gravity acceleration only");

        group.MapPost("/calculate/drag", CalculateDrag)
            .WithName("CalculateDrag")
            .WithSummary("Calculate atmospheric drag acceleration only");

        group.MapPost("/calculate/srp", CalculateSrp)
            .WithName("CalculateSrp")
            .WithSummary("Calculate solar radiation pressure acceleration only");

        group.MapPost("/eclipse/check", CheckEclipse)
            .WithName("CheckEclipse")
            .WithSummary("Check if spacecraft is in eclipse");

        group.MapGet("/atmosphere/density", GetAtmosphericDensity)
            .WithName("GetAtmosphericDensity")
            .WithSummary("Get atmospheric density at an altitude");

        return app;
    }

    // Configuration Handlers
    private static async Task<IResult> GetAllConfigurations(
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllConfigurationsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(ForceModelConfigurationResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetConfiguration(
        Guid id,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetConfigurationAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(ForceModelConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> CreateConfiguration(
        [FromBody] CreateConfigurationRequest request,
        [FromServices] ForceModelService service,
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
        return Results.Created($"/api/forcemodel/configurations/{result.Value!.Id}", ForceModelConfigurationResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> CreateStandardConfigurations(
        [FromQuery] string userId,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateStandardConfigurationsAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { CreatedCount = result.Value })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> UpdateGravitySettings(
        Guid id,
        [FromBody] UpdateGravitySettingsRequest request,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateGravitySettingsAsync(
            id, request.Enable, request.Model, request.Degree, request.Order, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(ForceModelConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> UpdateAtmosphereSettings(
        Guid id,
        [FromBody] UpdateAtmosphereSettingsRequest request,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAtmosphereSettingsAsync(
            id, request.Enable, request.Model, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(ForceModelConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> UpdateSrpSettings(
        Guid id,
        [FromBody] UpdateSrpSettingsRequest request,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateSrpSettingsAsync(
            id, request.Enable, request.Model, request.EnableEclipsing, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(ForceModelConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> UpdateThirdBodySettings(
        Guid id,
        [FromBody] UpdateThirdBodySettingsRequest request,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateThirdBodySettingsAsync(
            id, request.EnableSun, request.EnableMoon, request.EnablePlanets, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(ForceModelConfigurationResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> DeleteConfiguration(
        Guid id,
        [FromServices] ForceModelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteConfigurationAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error.Message);
    }

    // Calculation Handlers
    private static IResult CalculateAcceleration(
        [FromBody] CalculateAccelerationRequest request,
        [FromServices] ForceCalculationService service)
    {
        var state = new SpacecraftState(
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.Epoch);

        var props = new SpacecraftProperties(
            request.MassKg,
            request.DragCoefficient,
            request.DragAreaM2,
            request.SrpAreaM2,
            request.ReflectivityCoefficient);

        ThirdBodyPositions? thirdBodies = null;
        if (request.SunX.HasValue && request.SunY.HasValue && request.SunZ.HasValue)
        {
            thirdBodies = new ThirdBodyPositions
            {
                SunX = request.SunX.Value,
                SunY = request.SunY.Value,
                SunZ = request.SunZ.Value,
                MoonX = request.MoonX ?? 0,
                MoonY = request.MoonY ?? 0,
                MoonZ = request.MoonZ ?? 0
            };
        }

        var result = service.CalculateAcceleration(state, props, null, thirdBodies);

        return result.IsSuccess
            ? Results.Ok(ForceCalculationResponse.FromResult(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CalculateGravity(
        [FromBody] CalculateGravityRequest request,
        [FromServices] ForceCalculationService service)
    {
        var state = new SpacecraftState(
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.Epoch);

        var result = service.CalculateGravityOnly(state, request.Model);

        return result.IsSuccess
            ? Results.Ok(new AccelerationResponse(result.Value!.Ax, result.Value.Ay, result.Value.Az, result.Value.Magnitude))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CalculateDrag(
        [FromBody] CalculateDragRequest request,
        [FromServices] ForceCalculationService service)
    {
        var state = new SpacecraftState(
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.Epoch);

        var props = new SpacecraftProperties(
            request.MassKg,
            request.DragCoefficient,
            request.DragAreaM2,
            0, 0);

        var result = service.CalculateDragOnly(state, props, request.Model);

        return result.IsSuccess
            ? Results.Ok(new AccelerationResponse(result.Value!.Ax, result.Value.Ay, result.Value.Az, result.Value.Magnitude))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CalculateSrp(
        [FromBody] CalculateSrpRequest request,
        [FromServices] ForceCalculationService service)
    {
        var state = new SpacecraftState(
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.Epoch);

        var props = new SpacecraftProperties(
            request.MassKg,
            0, 0,
            request.SrpAreaM2,
            request.ReflectivityCoefficient);

        var result = service.CalculateSrpOnly(state, props, request.SunX, request.SunY, request.SunZ, request.CheckEclipse);

        return result.IsSuccess
            ? Results.Ok(new SrpResponse(
                result.Value!.Ax, result.Value.Ay, result.Value.Az,
                result.Value.Magnitude, result.Value.Eclipse.ToString(), result.Value.ShadowFactor))
            : Results.Problem(result.Error.Message);
    }

    private static IResult CheckEclipse(
        [FromBody] CheckEclipseRequest request,
        [FromServices] ForceCalculationService service)
    {
        var state = new SpacecraftState(
            request.X, request.Y, request.Z,
            0, 0, 0,
            request.Epoch);

        var result = service.CheckEclipse(state, request.SunX, request.SunY, request.SunZ);

        return result.IsSuccess
            ? Results.Ok(new { EclipseType = result.Value!.ToString() })
            : Results.Problem(result.Error.Message);
    }

    private static IResult GetAtmosphericDensity(
        [FromQuery] double altitudeM,
        [FromQuery] AtmosphereModelType model,
        [FromServices] ForceCalculationService service)
    {
        var result = service.GetAtmosphericDensity(altitudeM, model);

        return result.IsSuccess
            ? Results.Ok(new AtmosphericDensityResponse(
                result.Value!.DensityKgM3, result.Value.TemperatureK, result.Value.ScaleHeightM))
            : Results.Problem(result.Error.Message);
    }
}

// Request DTOs
public record CreateConfigurationRequest(
    string Name,
    string CreatedByUserId,
    string? Description = null,
    Guid? MissionId = null);

public record UpdateGravitySettingsRequest(
    bool Enable,
    GravityModelType Model,
    int Degree,
    int Order);

public record UpdateAtmosphereSettingsRequest(
    bool Enable,
    AtmosphereModelType Model);

public record UpdateSrpSettingsRequest(
    bool Enable,
    SrpModelType Model,
    bool EnableEclipsing);

public record UpdateThirdBodySettingsRequest(
    bool EnableSun,
    bool EnableMoon,
    bool EnablePlanets);

public record CalculateAccelerationRequest(
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    DateTime Epoch,
    double MassKg,
    double DragCoefficient,
    double DragAreaM2,
    double SrpAreaM2,
    double ReflectivityCoefficient,
    Guid? ConfigurationId = null,
    double? SunX = null, double? SunY = null, double? SunZ = null,
    double? MoonX = null, double? MoonY = null, double? MoonZ = null);

public record CalculateGravityRequest(
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    DateTime Epoch,
    GravityModelType Model = GravityModelType.J2Only);

public record CalculateDragRequest(
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    DateTime Epoch,
    double MassKg,
    double DragCoefficient,
    double DragAreaM2,
    AtmosphereModelType Model = AtmosphereModelType.Exponential);

public record CalculateSrpRequest(
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    DateTime Epoch,
    double MassKg,
    double SrpAreaM2,
    double ReflectivityCoefficient,
    double SunX, double SunY, double SunZ,
    bool CheckEclipse = true);

public record CheckEclipseRequest(
    double X, double Y, double Z,
    DateTime Epoch,
    double SunX, double SunY, double SunZ);

// Response DTOs
public record ForceModelConfigurationResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? MissionId,
    bool EnableCentralBodyGravity,
    string GravityModel,
    int GravityDegree,
    int GravityOrder,
    bool EnableAtmosphericDrag,
    string AtmosphereModel,
    bool EnableSolarRadiationPressure,
    string SrpModel,
    bool EnableEclipsing,
    bool EnableThirdBodySun,
    bool EnableThirdBodyMoon,
    bool EnableThirdBodyPlanets,
    bool EnableRelativisticCorrections,
    bool EnableSolidEarthTides,
    bool EnableOceanTides)
{
    public static ForceModelConfigurationResponse FromEntity(ForceModelConfiguration entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.MissionId,
        entity.EnableCentralBodyGravity,
        entity.GravityModel.ToString(),
        entity.GravityDegree,
        entity.GravityOrder,
        entity.EnableAtmosphericDrag,
        entity.AtmosphereModel.ToString(),
        entity.EnableSolarRadiationPressure,
        entity.SrpModel.ToString(),
        entity.EnableEclipsing,
        entity.EnableThirdBodySun,
        entity.EnableThirdBodyMoon,
        entity.EnableThirdBodyPlanets,
        entity.EnableRelativisticCorrections,
        entity.EnableSolidEarthTides,
        entity.EnableOceanTides);
}

public record AccelerationResponse(double Ax, double Ay, double Az, double Magnitude);

public record SrpResponse(
    double Ax, double Ay, double Az,
    double Magnitude,
    string EclipseType,
    double ShadowFactor);

public record AtmosphericDensityResponse(
    double DensityKgM3,
    double TemperatureK,
    double ScaleHeightM);

public record ForceCalculationResponse(
    AccelerationResponse TotalAcceleration,
    AccelerationResponse GravityAcceleration,
    AccelerationResponse DragAcceleration,
    SrpResponse SrpAcceleration,
    AccelerationResponse ThirdBodyAcceleration,
    DateTime Epoch,
    bool InEclipse)
{
    public static ForceCalculationResponse FromResult(ForceCalculationResult result) => new(
        new AccelerationResponse(result.TotalAcceleration.Ax, result.TotalAcceleration.Ay, result.TotalAcceleration.Az, result.TotalMagnitude),
        new AccelerationResponse(result.GravityAcceleration.Ax, result.GravityAcceleration.Ay, result.GravityAcceleration.Az, result.GravityMagnitude),
        new AccelerationResponse(result.DragAcceleration.Ax, result.DragAcceleration.Ay, result.DragAcceleration.Az, result.DragMagnitude),
        new SrpResponse(result.SrpAcceleration.Ax, result.SrpAcceleration.Ay, result.SrpAcceleration.Az, result.SrpMagnitude, result.SrpAcceleration.Eclipse.ToString(), result.SrpAcceleration.ShadowFactor),
        new AccelerationResponse(result.ThirdBodyAcceleration.Ax, result.ThirdBodyAcceleration.Ay, result.ThirdBodyAcceleration.Az, result.ThirdBodyMagnitude),
        result.Epoch,
        result.InEclipse);
}
