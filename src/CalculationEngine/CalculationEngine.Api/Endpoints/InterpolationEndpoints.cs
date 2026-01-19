using CalculationEngine.Core.Services;

namespace CalculationEngine.Api.Endpoints;

public static class InterpolationEndpoints
{
    public static void MapInterpolationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/interpolation")
            .WithTags("Interpolation");

        group.MapPost("/linear", async (InterpolationRequest request, InterpolationService service) =>
        {
            var result = service.LinearInterpolate(request.X, request.Y, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Value = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("LinearInterpolate");

        group.MapPost("/linear/batch", async (BatchInterpolationRequest request, InterpolationService service) =>
        {
            var result = service.LinearInterpolateBatch(request.X, request.Y, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Values = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("LinearInterpolateBatch");

        group.MapPost("/cubic-spline", async (InterpolationRequest request, InterpolationService service) =>
        {
            var result = service.CubicSplineInterpolate(request.X, request.Y, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Value = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CubicSplineInterpolate");

        group.MapPost("/cubic-spline/batch", async (BatchInterpolationRequest request, InterpolationService service) =>
        {
            var result = service.CubicSplineInterpolateBatch(request.X, request.Y, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Values = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CubicSplineInterpolateBatch");

        group.MapPost("/hermite", async (HermiteInterpolationRequest request, InterpolationService service) =>
        {
            var result = service.HermiteInterpolate(request.X, request.Y, request.Dy, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Value = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("HermiteInterpolate");

        group.MapPost("/hermite/batch", async (BatchHermiteInterpolationRequest request, InterpolationService service) =>
        {
            var result = service.HermiteInterpolateBatch(request.X, request.Y, request.Dy, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Values = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("HermiteInterpolateBatch");

        group.MapPost("/polynomial", async (InterpolationRequest request, InterpolationService service) =>
        {
            var result = service.PolynomialInterpolate(request.X, request.Y, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Value = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("PolynomialInterpolate");

        group.MapPost("/akima", async (InterpolationRequest request, InterpolationService service) =>
        {
            var result = service.AkimaInterpolate(request.X, request.Y, request.Xi);
            return result.IsSuccess
                ? Results.Ok(new { Value = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("AkimaInterpolate");
    }
}

public sealed class InterpolationRequest
{
    public double[] X { get; init; } = Array.Empty<double>();
    public double[] Y { get; init; } = Array.Empty<double>();
    public double Xi { get; init; }
}

public sealed class BatchInterpolationRequest
{
    public double[] X { get; init; } = Array.Empty<double>();
    public double[] Y { get; init; } = Array.Empty<double>();
    public double[] Xi { get; init; } = Array.Empty<double>();
}

public sealed class HermiteInterpolationRequest
{
    public double[] X { get; init; } = Array.Empty<double>();
    public double[] Y { get; init; } = Array.Empty<double>();
    public double[] Dy { get; init; } = Array.Empty<double>();
    public double Xi { get; init; }
}

public sealed class BatchHermiteInterpolationRequest
{
    public double[] X { get; init; } = Array.Empty<double>();
    public double[] Y { get; init; } = Array.Empty<double>();
    public double[] Dy { get; init; } = Array.Empty<double>();
    public double[] Xi { get; init; } = Array.Empty<double>();
}
