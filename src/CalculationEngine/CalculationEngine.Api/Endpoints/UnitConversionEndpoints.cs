using CalculationEngine.Core.Services;

namespace CalculationEngine.Api.Endpoints;

public static class UnitConversionEndpoints
{
    public static void MapUnitConversionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/units")
            .WithTags("Unit Conversion");

        group.MapPost("/convert/length", async (UnitConversionRequest request, UnitConversionService service) =>
        {
            var result = service.ConvertLength(request.Value, request.FromUnit, request.ToUnit);
            return result.IsSuccess
                ? Results.Ok(new { Result = result.Value, FromUnit = request.FromUnit, ToUnit = request.ToUnit })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConvertLength");

        group.MapPost("/convert/mass", async (UnitConversionRequest request, UnitConversionService service) =>
        {
            var result = service.ConvertMass(request.Value, request.FromUnit, request.ToUnit);
            return result.IsSuccess
                ? Results.Ok(new { Result = result.Value, FromUnit = request.FromUnit, ToUnit = request.ToUnit })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConvertMass");

        group.MapPost("/convert/time", async (UnitConversionRequest request, UnitConversionService service) =>
        {
            var result = service.ConvertTime(request.Value, request.FromUnit, request.ToUnit);
            return result.IsSuccess
                ? Results.Ok(new { Result = result.Value, FromUnit = request.FromUnit, ToUnit = request.ToUnit })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConvertTime");

        group.MapPost("/convert/angle", async (UnitConversionRequest request, UnitConversionService service) =>
        {
            var result = service.ConvertAngle(request.Value, request.FromUnit, request.ToUnit);
            return result.IsSuccess
                ? Results.Ok(new { Result = result.Value, FromUnit = request.FromUnit, ToUnit = request.ToUnit })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConvertAngle");

        group.MapPost("/convert/velocity", async (UnitConversionRequest request, UnitConversionService service) =>
        {
            var result = service.ConvertVelocity(request.Value, request.FromUnit, request.ToUnit);
            return result.IsSuccess
                ? Results.Ok(new { Result = result.Value, FromUnit = request.FromUnit, ToUnit = request.ToUnit })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConvertVelocity");

        group.MapPost("/convert/force", async (UnitConversionRequest request, UnitConversionService service) =>
        {
            var result = service.ConvertForce(request.Value, request.FromUnit, request.ToUnit);
            return result.IsSuccess
                ? Results.Ok(new { Result = result.Value, FromUnit = request.FromUnit, ToUnit = request.ToUnit })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConvertForce");

        group.MapGet("/supported/{unitType}", (string unitType, UnitConversionService service) =>
        {
            var units = service.GetSupportedUnits(unitType);
            return Results.Ok(new { UnitType = unitType, SupportedUnits = units });
        }).WithName("GetSupportedUnits");
    }
}

public sealed class UnitConversionRequest
{
    public double Value { get; init; }
    public string FromUnit { get; init; } = string.Empty;
    public string ToUnit { get; init; } = string.Empty;
}
