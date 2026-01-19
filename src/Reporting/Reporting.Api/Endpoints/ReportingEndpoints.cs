using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Reporting.Core.Models;
using Reporting.Core.Services;

namespace Reporting.Api.Endpoints;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reporting")
            .RequireAuthorization();

        // Mission Reports
        group.MapPost("/mission/{missionId:guid}", GenerateMissionReport)
            .WithName("GenerateMissionReport")
            .WithSummary("Generate a comprehensive mission report");

        // State Vector Export
        group.MapGet("/export/states/{spacecraftId:guid}", ExportStateVectors)
            .WithName("ExportStateVectors")
            .WithSummary("Export state vectors to file");

        group.MapPost("/export/states", ExportStateVectorsCustom)
            .WithName("ExportStateVectorsCustom")
            .WithSummary("Export state vectors with custom parameters");

        // Orbital Elements Export
        group.MapGet("/export/elements/{spacecraftId:guid}", ExportOrbitalElements)
            .WithName("ExportOrbitalElements")
            .WithSummary("Export orbital elements to file");

        // TLE Generation
        group.MapGet("/tle/{spacecraftId:guid}", GenerateTle)
            .WithName("GenerateTle")
            .WithSummary("Generate Two-Line Element set");

        group.MapPost("/tle", GenerateTleCustom)
            .WithName("GenerateTleCustom")
            .WithSummary("Generate TLE with custom orbital state");

        // Delta-V Budget
        group.MapGet("/delta-v/{missionId:guid}", GetDeltaVBudget)
            .WithName("GetDeltaVBudget")
            .WithSummary("Get delta-V budget report");

        group.MapPost("/delta-v", GenerateDeltaVBudget)
            .WithName("GenerateDeltaVBudget")
            .WithSummary("Generate delta-V budget with custom data");

        // Event Timeline
        group.MapGet("/timeline/{missionId:guid}", GetEventTimeline)
            .WithName("GetEventTimeline")
            .WithSummary("Get event timeline report");

        // Conjunction Report
        group.MapGet("/conjunction/{spacecraftId:guid}", GetConjunctionReport)
            .WithName("GetConjunctionReport")
            .WithSummary("Get conjunction analysis report");

        group.MapPost("/conjunction", GenerateConjunctionReport)
            .WithName("GenerateConjunctionReport")
            .WithSummary("Generate conjunction report with custom parameters");

        // Report formats
        group.MapGet("/formats", GetAvailableFormats)
            .WithName("GetReportFormats")
            .WithSummary("Get available report output formats");

        return app;
    }

    // Mission Report Handlers

    private static IResult GenerateMissionReport(
        Guid missionId,
        [FromQuery] string? format,
        [FromQuery] string? name,
        [FromServices] ReportingService service)
    {
        var reportFormat = ParseFormat(format);
        var missionName = name ?? "Unnamed Mission";

        var result = service.GenerateMissionReport(missionId, missionName, reportFormat);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        var report = result.Value!;
        return Results.File(
            report.Content ?? Array.Empty<byte>(),
            report.ContentType,
            report.FileName);
    }

    // State Vector Export Handlers

    private static IResult ExportStateVectors(
        Guid spacecraftId,
        [FromQuery] string? format,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? intervalSeconds,
        [FromQuery] string? coordinateSystem,
        [FromServices] ReportingService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = intervalSeconds ?? 60;
        var coordSys = coordinateSystem ?? "J2000";
        var reportFormat = ParseFormat(format);

        var result = service.ExportStateVectors(spacecraftId, start, end, interval, reportFormat, coordSys);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        var export = result.Value!;
        var bytes = service.ExportToBytes(export);
        var contentType = FormatToContentType(reportFormat);
        var fileName = $"state_vectors_{spacecraftId:N}.{FormatToExtension(reportFormat)}";

        return Results.File(bytes, contentType, fileName);
    }

    private static IResult ExportStateVectorsCustom(
        [FromBody] StateVectorExportRequest request,
        [FromServices] ReportingService service)
    {
        var result = service.ExportStateVectors(
            request.SpacecraftId,
            request.StartEpoch,
            request.EndEpoch,
            request.SampleIntervalSeconds,
            request.Format,
            request.CoordinateSystem,
            request.InitialState);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        var export = result.Value!;
        var bytes = service.ExportToBytes(export);
        var contentType = FormatToContentType(request.Format);
        var fileName = $"state_vectors_{request.SpacecraftId:N}.{FormatToExtension(request.Format)}";

        return Results.File(bytes, contentType, fileName);
    }

    // Orbital Elements Export Handlers

    private static IResult ExportOrbitalElements(
        Guid spacecraftId,
        [FromQuery] string? format,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] double? intervalSeconds,
        [FromServices] ReportingService service)
    {
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(2);
        var interval = intervalSeconds ?? 60;
        var reportFormat = ParseFormat(format);

        var result = service.ExportOrbitalElements(spacecraftId, start, end, interval, reportFormat);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        var export = result.Value!;
        var bytes = service.ExportToBytes(export);
        var contentType = FormatToContentType(reportFormat);
        var fileName = $"orbital_elements_{spacecraftId:N}.{FormatToExtension(reportFormat)}";

        return Results.File(bytes, contentType, fileName);
    }

    // TLE Generation Handlers

    private static IResult GenerateTle(
        Guid spacecraftId,
        [FromQuery] string? name,
        [FromQuery] DateTime? epoch,
        [FromQuery] int? noradNumber,
        [FromServices] ReportingService service)
    {
        var tleEpoch = epoch ?? DateTime.UtcNow;
        var scName = name ?? $"SC-{spacecraftId.ToString()[..8]}";

        var result = service.GenerateTle(spacecraftId, scName, tleEpoch, null, noradNumber);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(new TleResponse(result.Value!));
    }

    private static IResult GenerateTleCustom(
        [FromBody] TleGenerationRequest request,
        [FromServices] ReportingService service)
    {
        var result = service.GenerateTle(
            request.SpacecraftId,
            request.SpacecraftName,
            request.Epoch,
            request.State,
            request.NoradNumber);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(new TleResponse(result.Value!));
    }

    // Delta-V Budget Handlers

    private static IResult GetDeltaVBudget(
        Guid missionId,
        [FromQuery] string? name,
        [FromServices] ReportingService service)
    {
        var missionName = name ?? "Unknown Mission";

        // Default values for demonstration
        var result = service.GenerateDeltaVBudget(missionId, missionName, 1000, 800);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(result.Value);
    }

    private static IResult GenerateDeltaVBudget(
        [FromBody] DeltaVBudgetRequest request,
        [FromServices] ReportingService service)
    {
        var maneuvers = request.Maneuvers?.Select((m, i) => new DeltaVManeuver
        {
            Sequence = i + 1,
            Name = m.Name,
            Epoch = m.Epoch,
            Type = m.Type,
            DeltaVMps = m.DeltaVMps,
            FuelUsedKg = m.FuelUsedKg,
            Status = m.Status
        }).ToList();

        var result = service.GenerateDeltaVBudget(
            request.MissionId,
            request.MissionName,
            request.InitialFuelKg,
            request.RemainingFuelKg,
            maneuvers);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(result.Value);
    }

    // Event Timeline Handler

    private static IResult GetEventTimeline(
        Guid missionId,
        [FromQuery] string? name,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] string? format,
        [FromServices] ReportingService service)
    {
        var missionName = name ?? "Unknown Mission";
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(24);
        var reportFormat = ParseFormat(format);

        var result = service.GenerateEventTimeline(missionId, missionName, start, end, reportFormat);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(result.Value);
    }

    // Conjunction Report Handlers

    private static IResult GetConjunctionReport(
        Guid spacecraftId,
        [FromQuery] string? name,
        [FromQuery] DateTime? startEpoch,
        [FromQuery] DateTime? endEpoch,
        [FromQuery] string? format,
        [FromServices] ReportingService service)
    {
        var scName = name ?? $"SC-{spacecraftId.ToString()[..8]}";
        var start = startEpoch ?? DateTime.UtcNow;
        var end = endEpoch ?? start.AddHours(24);
        var reportFormat = ParseFormat(format);

        var result = service.GenerateConjunctionReport(spacecraftId, scName, start, end, reportFormat);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(result.Value);
    }

    private static IResult GenerateConjunctionReport(
        [FromBody] ConjunctionReportRequest request,
        [FromServices] ReportingService service)
    {
        var result = service.GenerateConjunctionReport(
            request.SpacecraftId,
            request.SpacecraftName,
            request.StartEpoch,
            request.EndEpoch,
            request.Format,
            request.PrimaryState);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        return Results.Ok(result.Value);
    }

    // Available Formats

    private static IResult GetAvailableFormats()
    {
        var formats = new[]
        {
            new FormatInfo("pdf", "PDF", "Portable Document Format - for printable reports"),
            new FormatInfo("html", "HTML", "HyperText Markup Language - for web viewing"),
            new FormatInfo("markdown", "Markdown", "Markdown format - for documentation"),
            new FormatInfo("csv", "CSV", "Comma-Separated Values - for spreadsheets"),
            new FormatInfo("json", "JSON", "JavaScript Object Notation - for data interchange"),
            new FormatInfo("xml", "XML", "eXtensible Markup Language - for structured data")
        };

        return Results.Ok(formats);
    }

    // Helper methods

    private static ReportFormat ParseFormat(string? format) => format?.ToLower() switch
    {
        "pdf" => ReportFormat.Pdf,
        "html" => ReportFormat.Html,
        "markdown" or "md" => ReportFormat.Markdown,
        "csv" => ReportFormat.Csv,
        "json" => ReportFormat.Json,
        "xml" => ReportFormat.Xml,
        _ => ReportFormat.Json
    };

    private static string FormatToContentType(ReportFormat format) => format switch
    {
        ReportFormat.Pdf => "application/pdf",
        ReportFormat.Html => "text/html",
        ReportFormat.Markdown => "text/markdown",
        ReportFormat.Csv => "text/csv",
        ReportFormat.Json => "application/json",
        ReportFormat.Xml => "application/xml",
        _ => "application/octet-stream"
    };

    private static string FormatToExtension(ReportFormat format) => format switch
    {
        ReportFormat.Pdf => "pdf",
        ReportFormat.Html => "html",
        ReportFormat.Markdown => "md",
        ReportFormat.Csv => "csv",
        ReportFormat.Json => "json",
        ReportFormat.Xml => "xml",
        _ => "txt"
    };
}

// Request DTOs

public record StateVectorExportRequest(
    Guid SpacecraftId,
    DateTime StartEpoch,
    DateTime EndEpoch,
    double SampleIntervalSeconds = 60,
    ReportFormat Format = ReportFormat.Csv,
    string CoordinateSystem = "J2000",
    double[]? InitialState = null);

public record TleGenerationRequest(
    Guid SpacecraftId,
    string SpacecraftName,
    DateTime Epoch,
    double[]? State = null,
    int? NoradNumber = null);

public record DeltaVBudgetRequest(
    Guid MissionId,
    string MissionName,
    double InitialFuelKg,
    double RemainingFuelKg,
    List<ManeuverInput>? Maneuvers = null);

public record ManeuverInput(
    string Name,
    DateTime Epoch,
    string Type,
    double DeltaVMps,
    double FuelUsedKg,
    string Status = "Planned");

public record ConjunctionReportRequest(
    Guid SpacecraftId,
    string SpacecraftName,
    DateTime StartEpoch,
    DateTime EndEpoch,
    ReportFormat Format = ReportFormat.Json,
    double[]? PrimaryState = null);

// Response DTOs

public record TleResponse(
    Guid Id,
    Guid SpacecraftId,
    string SpacecraftName,
    int? NoradCatalogNumber,
    DateTime Epoch,
    string Line1,
    string Line2,
    double InclinationDeg,
    double RaanDeg,
    double Eccentricity,
    double ArgPerigeeDeg,
    double MeanAnomalyDeg,
    double MeanMotionRevPerDay)
{
    public TleResponse(TleData tle) : this(
        tle.Id,
        tle.SpacecraftId,
        tle.SpacecraftName,
        tle.NoradCatalogNumber,
        tle.Epoch,
        tle.Line1,
        tle.Line2,
        tle.InclinationDeg,
        tle.RaanDeg,
        tle.Eccentricity,
        tle.ArgPerigeeDeg,
        tle.MeanAnomalyDeg,
        tle.MeanMotionRevPerDay)
    { }
}

public record FormatInfo(
    string Name,
    string DisplayName,
    string Description);
