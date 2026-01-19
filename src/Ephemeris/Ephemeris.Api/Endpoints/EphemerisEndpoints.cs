using Microsoft.AspNetCore.Mvc;
using Ephemeris.Core.Entities;
using Ephemeris.Core.Services;

namespace Ephemeris.Api.Endpoints;

public static class EphemerisEndpoints
{
    public static IEndpointRouteBuilder MapEphemerisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ephemeris")
            .WithTags("Ephemeris")
            .RequireAuthorization();

        // Celestial Bodies
        group.MapGet("/bodies", GetAllCelestialBodies)
            .WithName("GetAllCelestialBodies")
            .WithSummary("Get all celestial bodies");

        group.MapGet("/bodies/{id:guid}", GetCelestialBody)
            .WithName("GetCelestialBody")
            .WithSummary("Get a celestial body by ID");

        group.MapGet("/bodies/naif/{naifId:int}", GetCelestialBodyByNaifId)
            .WithName("GetCelestialBodyByNaifId")
            .WithSummary("Get a celestial body by NAIF ID");

        group.MapPost("/bodies", CreateCelestialBody)
            .WithName("CreateCelestialBody")
            .WithSummary("Create a new celestial body");

        group.MapPost("/bodies/import-standard", ImportStandardBodies)
            .WithName("ImportStandardBodies")
            .WithSummary("Import standard celestial bodies (Sun, planets, Moon)");

        // Celestial Body Positions
        group.MapGet("/positions/{celestialBodyId:guid}", GetPositionAtEpoch)
            .WithName("GetCelestialBodyPosition")
            .WithSummary("Get celestial body position at a specific epoch");

        group.MapGet("/positions/naif/{naifId:int}", GetPositionByNaifIdAtEpoch)
            .WithName("GetCelestialBodyPositionByNaifId")
            .WithSummary("Get celestial body position by NAIF ID at a specific epoch");

        group.MapPost("/positions", RecordPosition)
            .WithName("RecordCelestialBodyPosition")
            .WithSummary("Record a new celestial body position");

        group.MapGet("/positions/{celestialBodyId:guid}/range", GetPositionsInRange)
            .WithName("GetCelestialBodyPositionsInRange")
            .WithSummary("Get celestial body positions in a time range");

        // Earth Orientation Parameters
        group.MapGet("/eop", GetEopAtDate)
            .WithName("GetEarthOrientationParameters")
            .WithSummary("Get Earth orientation parameters for a date");

        group.MapGet("/eop/range", GetEopInRange)
            .WithName("GetEarthOrientationParametersInRange")
            .WithSummary("Get Earth orientation parameters in a date range");

        group.MapPost("/eop", RecordEop)
            .WithName("RecordEarthOrientationParameters")
            .WithSummary("Record new Earth orientation parameters");

        group.MapGet("/eop/ut1-utc", GetUt1MinusUtc)
            .WithName("GetUT1MinusUTC")
            .WithSummary("Get UT1-UTC difference for a date");

        group.MapGet("/eop/polar-motion", GetPolarMotion)
            .WithName("GetPolarMotion")
            .WithSummary("Get polar motion coordinates for a date");

        // Space Weather
        group.MapGet("/weather", GetSpaceWeather)
            .WithName("GetSpaceWeather")
            .WithSummary("Get space weather data for a date");

        group.MapGet("/weather/range", GetSpaceWeatherInRange)
            .WithName("GetSpaceWeatherInRange")
            .WithSummary("Get space weather data in a date range");

        group.MapPost("/weather", RecordSpaceWeather)
            .WithName("RecordSpaceWeather")
            .WithSummary("Record new space weather data");

        group.MapGet("/weather/atmospheric-indices", GetAtmosphericIndices)
            .WithName("GetAtmosphericIndices")
            .WithSummary("Get atmospheric indices (F10.7, Ap, Kp) for a date");

        // Time Conversion
        group.MapGet("/time/tai-minus-utc", GetTaiMinusUtc)
            .WithName("GetTAIMinusUTC")
            .WithSummary("Get TAI-UTC (leap seconds) for a date");

        group.MapGet("/time/convert", ConvertTime)
            .WithName("ConvertTime")
            .WithSummary("Convert between time systems (UTC, TAI, TT, UT1)");

        group.MapGet("/time/julian-date", GetJulianDate)
            .WithName("GetJulianDate")
            .WithSummary("Convert DateTime to Julian Date");

        group.MapGet("/time/leap-seconds", GetAllLeapSeconds)
            .WithName("GetAllLeapSeconds")
            .WithSummary("Get all leap seconds");

        group.MapPost("/time/leap-seconds/import-historical", ImportHistoricalLeapSeconds)
            .WithName("ImportHistoricalLeapSeconds")
            .WithSummary("Import historical leap seconds from IERS");

        return app;
    }

    // Celestial Bodies Handlers
    private static async Task<IResult> GetAllCelestialBodies(
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllCelestialBodiesAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(CelestialBodyResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetCelestialBody(
        Guid id,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCelestialBodyAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(CelestialBodyResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetCelestialBodyByNaifId(
        int naifId,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCelestialBodyByNaifIdAsync(naifId, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(CelestialBodyResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> CreateCelestialBody(
        [FromBody] CreateCelestialBodyRequest request,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateCelestialBodyAsync(
            request.Name, request.NaifId, request.Type, request.GravitationalParameterM3S2,
            request.MeanRadiusKm, request.EquatorialRadiusKm, request.PolarRadiusKm,
            request.FlatteningCoefficient, request.J2Coefficient, request.RotationPeriodSeconds,
            request.ParentBodyId, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("Conflict")
                ? Results.Conflict(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Created($"/api/ephemeris/bodies/{result.Value!.Id}", CelestialBodyResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> ImportStandardBodies(
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ImportStandardBodiesAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { ImportedCount = result.Value })
            : Results.Problem(result.Error.Message);
    }

    // Position Handlers
    private static async Task<IResult> GetPositionAtEpoch(
        Guid celestialBodyId,
        [FromQuery] DateTime epoch,
        [FromQuery] int centerNaifId,
        [FromQuery] bool interpolate,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPositionAtEpochAsync(celestialBodyId, epoch, centerNaifId, interpolate, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(CelestialBodyPositionResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetPositionByNaifIdAtEpoch(
        int naifId,
        [FromQuery] DateTime epoch,
        [FromQuery] int centerNaifId,
        [FromQuery] bool interpolate,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPositionByNaifIdAtEpochAsync(naifId, epoch, centerNaifId, interpolate, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(CelestialBodyPositionResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> RecordPosition(
        [FromBody] RecordPositionRequest request,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RecordPositionAsync(
            request.CelestialBodyId, request.Epoch,
            request.X, request.Y, request.Z,
            request.Vx, request.Vy, request.Vz,
            request.CenterNaifId, request.Source,
            request.ReferenceFrame, request.Ax, request.Ay, request.Az,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Created($"/api/ephemeris/positions/{result.Value!.Id}", CelestialBodyPositionResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> GetPositionsInRange(
        Guid celestialBodyId,
        [FromQuery] DateTime startEpoch,
        [FromQuery] DateTime endEpoch,
        [FromQuery] int centerNaifId,
        [FromServices] EphemerisService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPositionsInRangeAsync(celestialBodyId, startEpoch, endEpoch, centerNaifId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(CelestialBodyPositionResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    // EOP Handlers
    private static async Task<IResult> GetEopAtDate(
        [FromQuery] DateTime date,
        [FromQuery] bool interpolate,
        [FromServices] EarthOrientationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetEopAtDateAsync(date, interpolate, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(EopResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetEopInRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromServices] EarthOrientationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetEopInRangeAsync(startDate, endDate, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(EopResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> RecordEop(
        [FromBody] RecordEopRequest request,
        [FromServices] EarthOrientationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RecordEopAsync(
            request.Mjd, request.Date, request.XPoleArcsec, request.YPoleArcsec,
            request.Ut1MinusUtcSeconds, request.LodMilliseconds, request.DPsiArcsec,
            request.DEpsilonArcsec, request.Source, request.IsPrediction,
            request.XPoleUncertainty, request.YPoleUncertainty, request.Ut1MinusUtcUncertainty,
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/ephemeris/eop", EopResponse.FromEntity(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetUt1MinusUtc(
        [FromQuery] DateTime utc,
        [FromServices] EarthOrientationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetUt1MinusUtcAsync(utc, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { Utc = utc, Ut1MinusUtcSeconds = result.Value })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetPolarMotion(
        [FromQuery] DateTime utc,
        [FromServices] EarthOrientationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPolarMotionAsync(utc, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { Utc = utc, XPoleArcsec = result.Value.xPole, YPoleArcsec = result.Value.yPole })
            : Results.Problem(result.Error.Message);
    }

    // Space Weather Handlers
    private static async Task<IResult> GetSpaceWeather(
        [FromQuery] DateTime date,
        [FromQuery] bool interpolate,
        [FromServices] SpaceWeatherService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetSpaceWeatherAtDateAsync(date, interpolate, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(SpaceWeatherResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetSpaceWeatherInRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromServices] SpaceWeatherService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetSpaceWeatherInRangeAsync(startDate, endDate, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(SpaceWeatherResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> RecordSpaceWeather(
        [FromBody] RecordSpaceWeatherRequest request,
        [FromServices] SpaceWeatherService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RecordSpaceWeatherAsync(
            request.Date, request.F107Observed, request.F107Adjusted, request.F107Average81Day,
            request.ApDaily, request.KpSum, request.Source, request.IsPrediction,
            request.Ap3Hour, request.Kp3Hour, request.SunspotNumber, request.MgIiIndex,
            request.S107, request.M107, request.Y107, request.DstIndex,
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/ephemeris/weather", SpaceWeatherResponse.FromEntity(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetAtmosphericIndices(
        [FromQuery] DateTime date,
        [FromServices] SpaceWeatherService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAtmosphericIndicesAsync(date, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error.Message);
    }

    // Time Conversion Handlers
    private static async Task<IResult> GetTaiMinusUtc(
        [FromQuery] DateTime utc,
        [FromServices] TimeConversionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetTaiMinusUtcAsync(utc, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { Utc = utc, TaiMinusUtcSeconds = result.Value })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> ConvertTime(
        [FromQuery] DateTime utc,
        [FromServices] TimeConversionService service,
        CancellationToken cancellationToken)
    {
        var taiResult = await service.UtcToTaiAsync(utc, cancellationToken);
        var ttResult = await service.UtcToTtAsync(utc, cancellationToken);
        var ut1Result = await service.UtcToUt1Async(utc, cancellationToken);
        var jdResult = service.DateTimeToJulianDate(utc);
        var mjdResult = service.DateTimeToModifiedJulianDate(utc);

        return Results.Ok(new
        {
            Utc = utc,
            Tai = taiResult.IsSuccess ? taiResult.Value : (DateTime?)null,
            Tt = ttResult.IsSuccess ? ttResult.Value : (DateTime?)null,
            Ut1 = ut1Result.IsSuccess ? ut1Result.Value : (DateTime?)null,
            JulianDate = jdResult.Value,
            ModifiedJulianDate = mjdResult.Value
        });
    }

    private static IResult GetJulianDate(
        [FromQuery] DateTime dateTime,
        [FromServices] TimeConversionService service)
    {
        var jdResult = service.DateTimeToJulianDate(dateTime);
        var mjdResult = service.DateTimeToModifiedJulianDate(dateTime);
        var centuriesResult = service.GetJulianCenturiesFromJ2000(dateTime);

        return Results.Ok(new
        {
            DateTime = dateTime,
            JulianDate = jdResult.Value,
            ModifiedJulianDate = mjdResult.Value,
            JulianCenturiesFromJ2000 = centuriesResult.Value
        });
    }

    private static async Task<IResult> GetAllLeapSeconds(
        [FromServices] TimeConversionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllLeapSecondsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(LeapSecondResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> ImportHistoricalLeapSeconds(
        [FromServices] TimeConversionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ImportHistoricalLeapSecondsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { ImportedCount = result.Value })
            : Results.Problem(result.Error.Message);
    }
}

// DTOs
public record CreateCelestialBodyRequest(
    string Name,
    int NaifId,
    CelestialBodyType Type,
    double GravitationalParameterM3S2,
    double MeanRadiusKm,
    double? EquatorialRadiusKm = null,
    double? PolarRadiusKm = null,
    double? FlatteningCoefficient = null,
    double? J2Coefficient = null,
    double? RotationPeriodSeconds = null,
    Guid? ParentBodyId = null);

public record RecordPositionRequest(
    Guid CelestialBodyId,
    DateTime Epoch,
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    int CenterNaifId,
    string Source,
    string ReferenceFrame = "ICRF",
    double? Ax = null, double? Ay = null, double? Az = null);

public record RecordEopRequest(
    double Mjd,
    DateTime Date,
    double XPoleArcsec,
    double YPoleArcsec,
    double Ut1MinusUtcSeconds,
    double LodMilliseconds,
    double DPsiArcsec,
    double DEpsilonArcsec,
    string Source,
    bool IsPrediction,
    double? XPoleUncertainty = null,
    double? YPoleUncertainty = null,
    double? Ut1MinusUtcUncertainty = null);

public record RecordSpaceWeatherRequest(
    DateTime Date,
    double F107Observed,
    double F107Adjusted,
    double F107Average81Day,
    double ApDaily,
    double KpSum,
    string Source,
    bool IsPrediction,
    double[]? Ap3Hour = null,
    double[]? Kp3Hour = null,
    double? SunspotNumber = null,
    double? MgIiIndex = null,
    double? S107 = null,
    double? M107 = null,
    double? Y107 = null,
    double? DstIndex = null);

public record CelestialBodyResponse(
    Guid Id,
    string Name,
    int NaifId,
    string Type,
    double GravitationalParameterM3S2,
    double MeanRadiusKm,
    double? EquatorialRadiusKm,
    double? PolarRadiusKm,
    double? FlatteningCoefficient,
    double? J2Coefficient,
    double? RotationPeriodSeconds,
    Guid? ParentBodyId,
    string? ParentBodyName)
{
    public static CelestialBodyResponse FromEntity(CelestialBody entity) => new(
        entity.Id,
        entity.Name,
        entity.NaifId,
        entity.Type.ToString(),
        entity.GravitationalParameterM3S2,
        entity.MeanRadiusKm,
        entity.EquatorialRadiusKm,
        entity.PolarRadiusKm,
        entity.FlatteningCoefficient,
        entity.J2Coefficient,
        entity.RotationPeriodSeconds,
        entity.ParentBodyId,
        entity.ParentBody?.Name);
}

public record CelestialBodyPositionResponse(
    Guid Id,
    Guid CelestialBodyId,
    string? CelestialBodyName,
    DateTime Epoch,
    double X, double Y, double Z,
    double Vx, double Vy, double Vz,
    double? Ax, double? Ay, double? Az,
    string ReferenceFrame,
    int CenterNaifId,
    string Source,
    double DistanceFromCenter,
    double Speed)
{
    public static CelestialBodyPositionResponse FromEntity(CelestialBodyPosition entity) => new(
        entity.Id,
        entity.CelestialBodyId,
        entity.CelestialBody?.Name,
        entity.Epoch,
        entity.X, entity.Y, entity.Z,
        entity.Vx, entity.Vy, entity.Vz,
        entity.Ax, entity.Ay, entity.Az,
        entity.ReferenceFrame,
        entity.CenterNaifId,
        entity.Source,
        entity.DistanceFromCenter,
        entity.Speed);
}

public record EopResponse(
    Guid Id,
    double Mjd,
    DateTime Date,
    double XPoleArcsec,
    double YPoleArcsec,
    double Ut1MinusUtcSeconds,
    double LodMilliseconds,
    double DPsiArcsec,
    double DEpsilonArcsec,
    string Source,
    bool IsPrediction)
{
    public static EopResponse FromEntity(EarthOrientationParameters entity) => new(
        entity.Id,
        entity.Mjd,
        entity.Date,
        entity.XPoleArcsec,
        entity.YPoleArcsec,
        entity.Ut1MinusUtcSeconds,
        entity.LodMilliseconds,
        entity.DPsiArcsec,
        entity.DEpsilonArcsec,
        entity.Source,
        entity.IsPrediction);
}

public record SpaceWeatherResponse(
    Guid Id,
    DateTime Date,
    double F107Observed,
    double F107Adjusted,
    double F107Average81Day,
    double ApDaily,
    double KpSum,
    double[]? Ap3Hour,
    double[]? Kp3Hour,
    double? SunspotNumber,
    string Source,
    bool IsPrediction)
{
    public static SpaceWeatherResponse FromEntity(SpaceWeatherData entity) => new(
        entity.Id,
        entity.Date,
        entity.F107Observed,
        entity.F107Adjusted,
        entity.F107Average81Day,
        entity.ApDaily,
        entity.KpSum,
        entity.Ap3Hour,
        entity.Kp3Hour,
        entity.SunspotNumber,
        entity.Source,
        entity.IsPrediction);
}

public record LeapSecondResponse(
    Guid Id,
    DateTime EffectiveDate,
    double TaiMinusUtcSeconds,
    string Source)
{
    public static LeapSecondResponse FromEntity(LeapSecond entity) => new(
        entity.Id,
        entity.EffectiveDate,
        entity.TaiMinusUtcSeconds,
        entity.Source);
}
