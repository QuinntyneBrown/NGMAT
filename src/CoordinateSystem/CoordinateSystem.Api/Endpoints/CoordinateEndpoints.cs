using CoordinateSystem.Core.Entities;
using CoordinateSystem.Core.Events;
using CoordinateSystem.Core.Interfaces;
using CoordinateSystem.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging.Abstractions;

namespace CoordinateSystem.Api.Endpoints;

public static class CoordinateEndpoints
{
    public static void MapCoordinateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/coordinates")
            .WithTags("Coordinates")
            .RequireAuthorization();

        // Reference Frame endpoints
        group.MapPost("/systems", CreateReferenceFrame)
            .WithName("CreateReferenceFrame")
            .WithSummary("Create a new coordinate reference frame")
            .Produces<ReferenceFrameResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/systems", GetAllReferenceFrames)
            .WithName("GetAllReferenceFrames")
            .WithSummary("Get all coordinate reference frames")
            .Produces<IReadOnlyList<ReferenceFrameResponse>>();

        group.MapGet("/systems/{id:guid}", GetReferenceFrameById)
            .WithName("GetReferenceFrameById")
            .WithSummary("Get a reference frame by ID")
            .Produces<ReferenceFrameResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/systems/builtin", GetBuiltInFrames)
            .WithName("GetBuiltInFrames")
            .WithSummary("Get all built-in reference frames")
            .Produces<IReadOnlyList<ReferenceFrameResponse>>();

        // Transformation endpoints
        group.MapPost("/transform", TransformStateVector)
            .WithName("TransformStateVector")
            .WithSummary("Transform a state vector between coordinate systems")
            .Produces<StateVectorResponse>();

        group.MapPost("/eci-to-ecef", EciToEcef)
            .WithName("EciToEcef")
            .WithSummary("Transform state vector from ECI J2000 to ECEF")
            .Produces<StateVectorResponse>();

        group.MapPost("/ecef-to-eci", EcefToEci)
            .WithName("EcefToEci")
            .WithSummary("Transform state vector from ECEF to ECI J2000")
            .Produces<StateVectorResponse>();

        group.MapPost("/ecef-to-geodetic", EcefToGeodetic)
            .WithName("EcefToGeodetic")
            .WithSummary("Convert ECEF position to geodetic coordinates")
            .Produces<GeodeticResponse>();

        group.MapPost("/geodetic-to-ecef", GeodeticToEcef)
            .WithName("GeodeticToEcef")
            .WithSummary("Convert geodetic coordinates to ECEF position")
            .Produces<Vector3Response>();

        // Body-fixed frame endpoints
        group.MapPost("/vnb-matrix", GetVnbMatrix)
            .WithName("GetVnbMatrix")
            .WithSummary("Get VNB (Velocity-Normal-Binormal) transformation matrix")
            .Produces<MatrixResponse>();

        group.MapPost("/lvlh-matrix", GetLvlhMatrix)
            .WithName("GetLvlhMatrix")
            .WithSummary("Get LVLH (Local Vertical Local Horizontal) transformation matrix")
            .Produces<MatrixResponse>();

        // Keplerian elements endpoints
        group.MapPost("/state-to-keplerian", StateToKeplerian)
            .WithName("StateToKeplerian")
            .WithSummary("Convert state vector to Keplerian elements")
            .Produces<KeplerianElementsResponse>();

        group.MapPost("/keplerian-to-state", KeplerianToState)
            .WithName("KeplerianToState")
            .WithSummary("Convert Keplerian elements to state vector")
            .Produces<StateVectorResponse>();

        // Julian date utilities
        group.MapGet("/julian-date", GetJulianDate)
            .WithName("GetJulianDate")
            .WithSummary("Convert datetime to Julian date")
            .Produces<JulianDateResponse>();

        group.MapGet("/gmst", GetGmst)
            .WithName("GetGmst")
            .WithSummary("Get Greenwich Mean Sidereal Time")
            .Produces<GmstResponse>();
    }

    private static async Task<IResult> CreateReferenceFrame(
        [FromBody] CreateReferenceFrameRequest request,
        [FromServices] ICoordinateSystemUnitOfWork unitOfWork,
        [FromServices] IEventPublisher eventPublisher,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var exists = await unitOfWork.ReferenceFrames.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            return Results.Conflict($"A reference frame with name '{request.Name}' already exists");
        }

        var frame = ReferenceFrame.Create(
            request.Name,
            request.Type,
            request.CentralBody,
            request.Axes,
            request.Origin,
            request.Epoch,
            request.Description,
            null);

        await unitOfWork.ReferenceFrames.AddAsync(frame, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new CoordinateSystemCreatedEvent
        {
            ReferenceFrameId = frame.Id,
            Name = frame.Name,
            Type = frame.Type,
            CentralBody = frame.CentralBody,
            Axes = frame.Axes,
            CreatedByUserId = null
        }, cancellationToken);

        return Results.Created($"/v1/coordinates/systems/{frame.Id}", ToResponse(frame));
    }

    private static async Task<IResult> GetAllReferenceFrames(
        [FromServices] ICoordinateSystemUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var frames = await unitOfWork.ReferenceFrames.GetAllAsync(cancellationToken);
        return Results.Ok(frames.Select(ToResponse).ToList());
    }

    private static async Task<IResult> GetReferenceFrameById(
        Guid id,
        [FromServices] ICoordinateSystemUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var frame = await unitOfWork.ReferenceFrames.GetByIdAsync(id, cancellationToken);
        if (frame == null)
        {
            return Results.NotFound($"Reference frame with ID '{id}' not found");
        }
        return Results.Ok(ToResponse(frame));
    }

    private static async Task<IResult> GetBuiltInFrames(
        [FromServices] ICoordinateSystemUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var frames = await unitOfWork.ReferenceFrames.GetBuiltInFramesAsync(cancellationToken);
        return Results.Ok(frames.Select(ToResponse).ToList());
    }

    private static IResult TransformStateVector(
        [FromBody] TransformStateRequest request,
        [FromServices] CoordinateTransformService service)
    {
        // For now, only support ECI to ECEF transformations
        if (request.SourceFrameId == BuiltInFrames.EciJ2000Id && request.TargetFrameId == BuiltInFrames.EcefId)
        {
            var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
            var result = service.EciToEcef(state, request.Epoch);
            return Results.Ok(ToResponse(result));
        }
        else if (request.SourceFrameId == BuiltInFrames.EcefId && request.TargetFrameId == BuiltInFrames.EciJ2000Id)
        {
            var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
            var result = service.EcefToEci(state, request.Epoch);
            return Results.Ok(ToResponse(result));
        }

        return Results.BadRequest("Unsupported frame transformation");
    }

    private static IResult EciToEcef(
        [FromBody] EciEcefRequest request,
        [FromServices] CoordinateTransformService service)
    {
        var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
        var result = service.EciToEcef(state, request.Epoch);
        return Results.Ok(ToResponse(result));
    }

    private static IResult EcefToEci(
        [FromBody] EciEcefRequest request,
        [FromServices] CoordinateTransformService service)
    {
        var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
        var result = service.EcefToEci(state, request.Epoch);
        return Results.Ok(ToResponse(result));
    }

    private static IResult EcefToGeodetic(
        [FromBody] Vector3Request request,
        [FromServices] CoordinateTransformService service)
    {
        var position = new Vector3(request.X, request.Y, request.Z);
        var result = service.EcefToGeodetic(position);
        return Results.Ok(new GeodeticResponse
        {
            LatitudeRadians = result.Latitude,
            LongitudeRadians = result.Longitude,
            AltitudeKm = result.Altitude,
            LatitudeDegrees = result.LatitudeDegrees,
            LongitudeDegrees = result.LongitudeDegrees
        });
    }

    private static IResult GeodeticToEcef(
        [FromBody] GeodeticRequest request,
        [FromServices] CoordinateTransformService service)
    {
        var geodetic = request.UseDegrees
            ? GeodeticCoordinates.FromDegrees(request.Latitude, request.Longitude, request.Altitude)
            : new GeodeticCoordinates(request.Latitude, request.Longitude, request.Altitude);

        var result = service.GeodeticToEcef(geodetic);
        return Results.Ok(new Vector3Response { X = result.X, Y = result.Y, Z = result.Z });
    }

    private static IResult GetVnbMatrix(
        [FromBody] StateVectorRequest request,
        [FromServices] CoordinateTransformService service)
    {
        var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
        var matrix = service.GetVnbMatrix(state);
        return Results.Ok(ToResponse(matrix));
    }

    private static IResult GetLvlhMatrix(
        [FromBody] StateVectorRequest request,
        [FromServices] CoordinateTransformService service)
    {
        var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
        var matrix = service.GetLvlhMatrix(state);
        return Results.Ok(ToResponse(matrix));
    }

    private static IResult StateToKeplerian(
        [FromBody] StateToKeplerianRequest request,
        [FromServices] OrbitalElementsService service)
    {
        var state = new StateVector(request.X, request.Y, request.Z, request.Vx, request.Vy, request.Vz);
        var elements = service.StateToKeplerian(state, request.Mu ?? Wgs84.GM);
        return Results.Ok(ToResponse(elements));
    }

    private static IResult KeplerianToState(
        [FromBody] KeplerianToStateRequest request,
        [FromServices] OrbitalElementsService service)
    {
        var elements = new KeplerianElements(
            request.SemiMajorAxis,
            request.Eccentricity,
            request.InclinationRadians,
            request.RaanRadians,
            request.ArgumentOfPeriapsisRadians,
            request.TrueAnomalyRadians,
            request.Mu ?? Wgs84.GM);

        var state = service.KeplerianToState(elements);
        return Results.Ok(ToResponse(state));
    }

    private static IResult GetJulianDate(
        [FromQuery] DateTime epoch,
        [FromServices] CoordinateTransformService service)
    {
        var jd = service.DateTimeToJulianDate(epoch);
        return Results.Ok(new JulianDateResponse { JulianDate = jd, DateTime = epoch });
    }

    private static IResult GetGmst(
        [FromQuery] DateTime epoch,
        [FromServices] CoordinateTransformService service)
    {
        var gmstRad = service.ComputeGmst(epoch);
        return Results.Ok(new GmstResponse
        {
            GmstRadians = gmstRad,
            GmstDegrees = gmstRad * 180.0 / Math.PI,
            GmstHours = gmstRad * 12.0 / Math.PI,
            Epoch = epoch
        });
    }

    // Response mapping helpers
    private static ReferenceFrameResponse ToResponse(ReferenceFrame frame) => new()
    {
        Id = frame.Id,
        Name = frame.Name,
        Description = frame.Description,
        Type = frame.Type.ToString(),
        CentralBody = frame.CentralBody.ToString(),
        Axes = frame.Axes.ToString(),
        Origin = frame.Origin.ToString(),
        Epoch = frame.Epoch,
        IsInertial = frame.IsInertial,
        IsBuiltIn = frame.IsBuiltIn,
        CreatedAt = frame.CreatedAt
    };

    private static StateVectorResponse ToResponse(StateVector state) => new()
    {
        X = state.X,
        Y = state.Y,
        Z = state.Z,
        Vx = state.Vx,
        Vy = state.Vy,
        Vz = state.Vz,
        PositionMagnitude = state.PositionMagnitude,
        VelocityMagnitude = state.VelocityMagnitude
    };

    private static MatrixResponse ToResponse(TransformationMatrix matrix) => new()
    {
        Elements = new[]
        {
            matrix.M11, matrix.M12, matrix.M13,
            matrix.M21, matrix.M22, matrix.M23,
            matrix.M31, matrix.M32, matrix.M33
        }
    };

    private static KeplerianElementsResponse ToResponse(KeplerianElements elements) => new()
    {
        SemiMajorAxisKm = elements.SemiMajorAxis,
        Eccentricity = elements.Eccentricity,
        InclinationRadians = elements.Inclination,
        InclinationDegrees = elements.Inclination * 180.0 / Math.PI,
        RaanRadians = elements.RAAN,
        RaanDegrees = elements.RAAN * 180.0 / Math.PI,
        ArgumentOfPeriapsisRadians = elements.ArgumentOfPeriapsis,
        ArgumentOfPerigapsisDegrees = elements.ArgumentOfPeriapsis * 180.0 / Math.PI,
        TrueAnomalyRadians = elements.TrueAnomaly,
        TrueAnomalyDegrees = elements.TrueAnomaly * 180.0 / Math.PI,
        Mu = elements.Mu,
        PeriodSeconds = elements.Period,
        ApoapsisRadiusKm = elements.ApoapsisRadius,
        PeriapsisRadiusKm = elements.PeriapsisRadius,
        IsCircular = elements.IsCircular,
        IsElliptical = elements.IsElliptical,
        IsParabolic = elements.IsParabolic,
        IsHyperbolic = elements.IsHyperbolic
    };
}

// Request/Response DTOs
public record CreateReferenceFrameRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReferenceFrameType Type { get; init; }
    public CentralBody CentralBody { get; init; }
    public AxesDefinition Axes { get; init; }
    public OriginDefinition Origin { get; init; }
    public DateTime? Epoch { get; init; }
}

public record ReferenceFrameResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string CentralBody { get; init; } = string.Empty;
    public string Axes { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public DateTime? Epoch { get; init; }
    public bool IsInertial { get; init; }
    public bool IsBuiltIn { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TransformStateRequest
{
    public Guid SourceFrameId { get; init; }
    public Guid TargetFrameId { get; init; }
    public DateTime Epoch { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
}

public record EciEcefRequest
{
    public DateTime Epoch { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
}

public record StateVectorRequest
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
}

public record StateVectorResponse
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
    public double PositionMagnitude { get; init; }
    public double VelocityMagnitude { get; init; }
}

public record Vector3Request
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public record Vector3Response
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public record GeodeticRequest
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Altitude { get; init; }
    public bool UseDegrees { get; init; } = true;
}

public record GeodeticResponse
{
    public double LatitudeRadians { get; init; }
    public double LongitudeRadians { get; init; }
    public double AltitudeKm { get; init; }
    public double LatitudeDegrees { get; init; }
    public double LongitudeDegrees { get; init; }
}

public record MatrixResponse
{
    public double[] Elements { get; init; } = Array.Empty<double>();
}

public record StateToKeplerianRequest
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
    public double? Mu { get; init; }
}

public record KeplerianToStateRequest
{
    public double SemiMajorAxis { get; init; }
    public double Eccentricity { get; init; }
    public double InclinationRadians { get; init; }
    public double RaanRadians { get; init; }
    public double ArgumentOfPeriapsisRadians { get; init; }
    public double TrueAnomalyRadians { get; init; }
    public double? Mu { get; init; }
}

public record KeplerianElementsResponse
{
    public double SemiMajorAxisKm { get; init; }
    public double Eccentricity { get; init; }
    public double InclinationRadians { get; init; }
    public double InclinationDegrees { get; init; }
    public double RaanRadians { get; init; }
    public double RaanDegrees { get; init; }
    public double ArgumentOfPeriapsisRadians { get; init; }
    public double ArgumentOfPerigapsisDegrees { get; init; }
    public double TrueAnomalyRadians { get; init; }
    public double TrueAnomalyDegrees { get; init; }
    public double Mu { get; init; }
    public double PeriodSeconds { get; init; }
    public double ApoapsisRadiusKm { get; init; }
    public double PeriapsisRadiusKm { get; init; }
    public bool IsCircular { get; init; }
    public bool IsElliptical { get; init; }
    public bool IsParabolic { get; init; }
    public bool IsHyperbolic { get; init; }
}

public record JulianDateResponse
{
    public double JulianDate { get; init; }
    public DateTime DateTime { get; init; }
}

public record GmstResponse
{
    public double GmstRadians { get; init; }
    public double GmstDegrees { get; init; }
    public double GmstHours { get; init; }
    public DateTime Epoch { get; init; }
}
