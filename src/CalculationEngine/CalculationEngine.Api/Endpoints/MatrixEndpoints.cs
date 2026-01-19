using CalculationEngine.Core.Services;

namespace CalculationEngine.Api.Endpoints;

public static class MatrixEndpoints
{
    public static void MapMatrixEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/matrix")
            .WithTags("Matrix Operations");

        group.MapPost("/add", async (MatrixOperationRequest request, MatrixService service) =>
        {
            var result = service.Add(request.A, request.B);
            return result.IsSuccess
                ? Results.Ok(new MatrixResult { Matrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("AddMatrices");

        group.MapPost("/subtract", async (MatrixOperationRequest request, MatrixService service) =>
        {
            var result = service.Subtract(request.A, request.B);
            return result.IsSuccess
                ? Results.Ok(new MatrixResult { Matrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SubtractMatrices");

        group.MapPost("/multiply", async (MatrixOperationRequest request, MatrixService service) =>
        {
            var result = service.Multiply(request.A, request.B);
            return result.IsSuccess
                ? Results.Ok(new MatrixResult { Matrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("MultiplyMatrices");

        group.MapPost("/transpose", async (SingleMatrixRequest request, MatrixService service) =>
        {
            var result = service.Transpose(request.Matrix);
            return result.IsSuccess
                ? Results.Ok(new MatrixResult { Matrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("TransposeMatrix");

        group.MapPost("/inverse", async (SingleMatrixRequest request, MatrixService service) =>
        {
            var result = service.Inverse(request.Matrix);
            return result.IsSuccess
                ? Results.Ok(new MatrixResult { Matrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("InverseMatrix");

        group.MapPost("/determinant", async (SingleMatrixRequest request, MatrixService service) =>
        {
            var result = service.Determinant(request.Matrix);
            return result.IsSuccess
                ? Results.Ok(new { Determinant = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("MatrixDeterminant");

        group.MapPost("/eigenvalues", async (SingleMatrixRequest request, MatrixService service) =>
        {
            var result = service.Eigenvalues(request.Matrix);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("MatrixEigenvalues");

        group.MapPost("/lu", async (SingleMatrixRequest request, MatrixService service) =>
        {
            var result = service.LuDecomposition(request.Matrix);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("LuDecomposition");

        group.MapPost("/solve", async (LinearSystemRequest request, MatrixService service) =>
        {
            var result = service.Solve(request.A, request.B);
            return result.IsSuccess
                ? Results.Ok(new { Solution = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SolveLinearSystem");
    }
}

public sealed class MatrixOperationRequest
{
    public double[][] A { get; init; } = Array.Empty<double[]>();
    public double[][] B { get; init; } = Array.Empty<double[]>();
}

public sealed class SingleMatrixRequest
{
    public double[][] Matrix { get; init; } = Array.Empty<double[]>();
}

public sealed class LinearSystemRequest
{
    public double[][] A { get; init; } = Array.Empty<double[]>();
    public double[] B { get; init; } = Array.Empty<double>();
}

public sealed class MatrixResult
{
    public double[][] Matrix { get; init; } = Array.Empty<double[]>();
}
