using Microsoft.AspNetCore.Mvc;
using Visualization.Core.Models;
using Visualization.Core.Services;

namespace Visualization.Api.Endpoints;

public static class VisualizationEndpoints
{
    public static IEndpointRouteBuilder MapVisualizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/visualization")
            .WithTags("Visualization")
            .RequireAuthorization();

        // 3D Orbit Plot
        group.MapGet("/orbit/{spacecraftId:guid}", GetOrbitPlot)
            .WithName("GetOrbitPlot")
            .WithSummary("Generate 3D orbit plot data for a spacecraft");

        group.MapPost("/orbit", GenerateOrbitPlot)
            .WithName("GenerateOrbitPlot")
            .WithSummary("Generate custom 3D orbit plot data");

        // Ground Track
        group.MapGet("/ground-track/{spacecraftId:guid}", GetGroundTrack)
            .WithName("GetGroundTrack")
            .WithSummary("Generate ground track (lat/lon) data for a spacecraft");

        group.MapPost("/ground-track", GenerateGroundTrack)
            .WithName("GenerateGroundTrack")
            .WithSummary("Generate custom ground track data");

        // Time Series
        group.MapGet("/timeseries/{parameter}", GetTimeSeries)
            .WithName("GetTimeSeries")
            .WithSummary("Generate time-series plot data for a parameter");

        group.MapPost("/timeseries", GenerateTimeSeries)
            .WithName("GenerateTimeSeries")
            .WithSummary("Generate custom time-series data");

        // Orbital Elements
        group.MapGet("/elements/{spacecraftId:guid}", GetOrbitalElements)
            .WithName("GetOrbitalElements")
            .WithSummary("Generate orbital elements plot data over time");

        group.MapPost("/elements", GenerateOrbitalElements)
            .WithName("GenerateOrbitalElements")
            .WithSummary("Generate custom orbital elements data");

        // Eclipse
        group.MapGet("/eclipse/{spacecraftId:guid}", GetEclipseData)
            .WithName("GetEclipseData")
            .WithSummary("Generate eclipse visualization data");

        group.MapPost("/eclipse", GenerateEclipseData)
            .WithName("GenerateEclipseData")
            .WithSummary("Generate custom eclipse data");

        // Attitude
        group.MapGet("/attitude/{spacecraftId:guid}", GetAttitudeData)
            .WithName("GetAttitudeData")
            .WithSummary("Generate attitude visualization data");

        group.MapPost("/attitude", GenerateAttitudeData)
            .WithName("GenerateAttitudeData")
            .WithSummary("Generate custom attitude data");

        // Conjunction Analysis
        group.MapGet("/conjunction", GetConjunctionData)
            .WithName("GetConjunctionData")
            .WithSummary("Generate conjunction analysis visualization");

        group.MapPost("/conjunction", GenerateConjunctionData)
            .WithName("GenerateConjunctionData")
            .WithSummary("Generate custom conjunction analysis");

        // Export
        group.MapGet("/export", ExportScene)
            .WithName("ExportScene")
            .WithSummary("Export 3D scene to GLTF/OBJ format");

        // Available parameters
        group.MapGet("/parameters", GetAvailableParameters)
            .WithName("GetVisualizationParameters")
            .WithSummary("Get list of available visualization parameters");

        return app;
    }

    // Orbit Plot Handlers

    private static IResult GetOrbitPlot(
        Guid spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateOrbitPlot(spacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateOrbitPlot(
        [FromBody] OrbitPlotRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateOrbitPlot(
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.InitialState,
            request.CentralBody);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Ground Track Handlers

    private static IResult GetGroundTrack(
        Guid spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateGroundTrack(spacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateGroundTrack(
        [FromBody] GroundTrackRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateGroundTrack(
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.InitialState);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Time Series Handlers

    private static IResult GetTimeSeries(
        string parameter,
        [FromQuery] Guid? spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateTimeSeries(parameter, spacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateTimeSeries(
        [FromBody] TimeSeriesRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateTimeSeries(
            request.ParameterName,
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.InitialState);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Orbital Elements Handlers

    private static IResult GetOrbitalElements(
        Guid spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateOrbitalElements(spacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateOrbitalElements(
        [FromBody] OrbitalElementsRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateOrbitalElements(
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.InitialState);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Eclipse Handlers

    private static IResult GetEclipseData(
        Guid spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateEclipseData(spacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateEclipseData(
        [FromBody] EclipseRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateEclipseData(
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.InitialState);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Attitude Handlers

    private static IResult GetAttitudeData(
        Guid spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateAttitudeData(spacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateAttitudeData(
        [FromBody] AttitudeRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateAttitudeData(
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.InitialState);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Conjunction Handlers

    private static IResult GetConjunctionData(
        [FromQuery] Guid primarySpacecraftId,
        [FromQuery] Guid secondarySpacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? sampleIntervalSeconds,
        [FromServices] VisualizationService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = sampleIntervalSeconds ?? 60;

        var result = service.GenerateConjunctionData(
            primarySpacecraftId, secondarySpacecraftId, start, end, interval);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    private static IResult GenerateConjunctionData(
        [FromBody] ConjunctionRequest request,
        [FromServices] VisualizationService service)
    {
        var result = service.GenerateConjunctionData(
            request.PrimarySpacecraftId,
            request.SecondarySpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.PrimaryInitialState,
            request.SecondaryInitialState);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Export Handler

    private static IResult ExportScene(
        [FromQuery] string format,
        [FromQuery] Guid? spacecraftId,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] bool includeCentralBody,
        [FromQuery] double? scaleFactor,
        [FromServices] VisualizationService visualizationService,
        [FromServices] SceneExportService exportService)
    {
        var exportFormat = format?.ToLower() switch
        {
            "gltf" or "glb" => ExportFormat.Gltf,
            "obj" => ExportFormat.Obj,
            _ => ExportFormat.Json
        };

        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var scId = spacecraftId ?? Guid.NewGuid();

        // Generate orbit data
        var orbitResult = visualizationService.GenerateOrbitPlot(scId, start, end, 60);
        if (!orbitResult.IsSuccess)
        {
            return Results.Problem(orbitResult.Error.Message);
        }

        // Export to requested format
        var result = exportService.ExportOrbit(
            orbitResult.Value,
            exportFormat,
            includeCentralBody,
            scaleFactor ?? 0.001);

        // Return file download or metadata based on format
        if (exportFormat == ExportFormat.Json)
        {
            return Results.Ok(new
            {
                result.Id,
                result.Format,
                result.FileName,
                result.MimeType,
                DataSizeBytes = result.Data.Length,
                result.GeneratedAt
            });
        }

        return Results.File(
            result.Data,
            result.MimeType,
            result.FileName);
    }

    // Parameters Handler

    private static IResult GetAvailableParameters()
    {
        var parameters = new[]
        {
            new ParameterInfo("altitude", "Altitude", "km", "Spacecraft altitude above Earth's surface"),
            new ParameterInfo("velocity", "Velocity Magnitude", "km/s", "Spacecraft velocity magnitude"),
            new ParameterInfo("x", "X Position", "km", "X component of position in ECI"),
            new ParameterInfo("y", "Y Position", "km", "Y component of position in ECI"),
            new ParameterInfo("z", "Z Position", "km", "Z component of position in ECI"),
            new ParameterInfo("vx", "X Velocity", "km/s", "X component of velocity in ECI"),
            new ParameterInfo("vy", "Y Velocity", "km/s", "Y component of velocity in ECI"),
            new ParameterInfo("vz", "Z Velocity", "km/s", "Z component of velocity in ECI"),
            new ParameterInfo("sma", "Semi-Major Axis", "km", "Orbital semi-major axis"),
            new ParameterInfo("ecc", "Eccentricity", "-", "Orbital eccentricity"),
            new ParameterInfo("inc", "Inclination", "deg", "Orbital inclination"),
            new ParameterInfo("raan", "RAAN", "deg", "Right Ascension of Ascending Node"),
            new ParameterInfo("aop", "Arg. of Periapsis", "deg", "Argument of Periapsis"),
            new ParameterInfo("ta", "True Anomaly", "deg", "True anomaly")
        };

        return Results.Ok(parameters);
    }
}

// Request DTOs

public record OrbitPlotRequest(
    Guid SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? InitialState = null,
    string CentralBody = "Earth");

public record GroundTrackRequest(
    Guid SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? InitialState = null);

public record TimeSeriesRequest(
    string ParameterName,
    Guid? SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? InitialState = null);

public record OrbitalElementsRequest(
    Guid SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? InitialState = null);

public record EclipseRequest(
    Guid SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? InitialState = null);

public record AttitudeRequest(
    Guid SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? InitialState = null);

public record ConjunctionRequest(
    Guid PrimarySpacecraftId,
    Guid SecondarySpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    double[]? PrimaryInitialState = null,
    double[]? SecondaryInitialState = null);

public record ParameterInfo(
    string Name,
    string DisplayName,
    string Unit,
    string Description);
