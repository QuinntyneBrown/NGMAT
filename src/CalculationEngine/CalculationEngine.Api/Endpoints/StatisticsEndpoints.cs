using CalculationEngine.Core.Services;

namespace CalculationEngine.Api.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statistics")
            .WithTags("Statistics");

        group.MapPost("/mean", async (ValuesRequest request, StatisticsService service) =>
        {
            var result = service.Mean(request.Values);
            return result.IsSuccess
                ? Results.Ok(new { Mean = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateMean");

        group.MapPost("/median", async (ValuesRequest request, StatisticsService service) =>
        {
            var result = service.Median(request.Values);
            return result.IsSuccess
                ? Results.Ok(new { Median = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateMedian");

        group.MapPost("/standard-deviation", async (ValuesRequest request, StatisticsService service) =>
        {
            var result = service.StandardDeviation(request.Values);
            return result.IsSuccess
                ? Results.Ok(new { StandardDeviation = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateStandardDeviation");

        group.MapPost("/variance", async (ValuesRequest request, StatisticsService service) =>
        {
            var result = service.Variance(request.Values);
            return result.IsSuccess
                ? Results.Ok(new { Variance = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateVariance");

        group.MapPost("/descriptive", async (ValuesRequest request, StatisticsService service) =>
        {
            var result = service.DescriptiveStatistics(request.Values);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("DescriptiveStatistics");

        group.MapPost("/percentiles", async (PercentilesRequest request, StatisticsService service) =>
        {
            var result = service.Percentiles(request.Values, request.Percentiles);
            return result.IsSuccess
                ? Results.Ok(new { Percentiles = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculatePercentiles");

        group.MapPost("/covariance", async (TwoArraysRequest request, StatisticsService service) =>
        {
            var result = service.Covariance(request.X, request.Y);
            return result.IsSuccess
                ? Results.Ok(new { Covariance = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateCovariance");

        group.MapPost("/correlation", async (TwoArraysRequest request, StatisticsService service) =>
        {
            var result = service.PearsonCorrelation(request.X, request.Y);
            return result.IsSuccess
                ? Results.Ok(new { Correlation = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateCorrelation");

        group.MapPost("/covariance-matrix", async (DataMatrixRequest request, StatisticsService service) =>
        {
            var result = service.CovarianceMatrix(request.Data);
            return result.IsSuccess
                ? Results.Ok(new { CovarianceMatrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateCovarianceMatrix");

        group.MapPost("/histogram", async (HistogramRequest request, StatisticsService service) =>
        {
            var result = service.Histogram(request.Values, request.Buckets);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("GenerateHistogram");
    }
}

public sealed class ValuesRequest
{
    public double[] Values { get; init; } = Array.Empty<double>();
}

public sealed class PercentilesRequest
{
    public double[] Values { get; init; } = Array.Empty<double>();
    public double[] Percentiles { get; init; } = Array.Empty<double>();
}

public sealed class TwoArraysRequest
{
    public double[] X { get; init; } = Array.Empty<double>();
    public double[] Y { get; init; } = Array.Empty<double>();
}

public sealed class DataMatrixRequest
{
    public double[][] Data { get; init; } = Array.Empty<double[]>();
}

public sealed class HistogramRequest
{
    public double[] Values { get; init; } = Array.Empty<double>();
    public int Buckets { get; init; } = 10;
}
