using CalculationEngine.Core.Services;

namespace CalculationEngine.Api.Endpoints;

public static class NumericalEndpoints
{
    public static void MapNumericalEndpoints(this IEndpointRouteBuilder app)
    {
        MapOdeEndpoints(app);
        MapDerivativeEndpoints(app);
    }

    private static void MapOdeEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ode")
            .WithTags("ODE Solvers");

        group.MapPost("/rk4", async (OdeSolveRequest request, OdeSolverService service) =>
        {
            // System derivative function based on linear coefficient matrix
            Func<double, double[], double[]> derivatives = (t, y) =>
                EvaluateLinearSystem(request.CoefficientMatrix, request.ConstantVector, t, y);

            var result = service.SolveRk4(
                derivatives,
                request.InitialState,
                request.T0,
                request.TEnd,
                request.StepSize);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SolveOdeRK4");

        group.MapPost("/rk45", async (OdeAdaptiveRequest request, OdeSolverService service) =>
        {
            Func<double, double[], double[]> derivatives = (t, y) =>
                EvaluateLinearSystem(request.CoefficientMatrix, request.ConstantVector, t, y);

            var result = service.SolveRk45(
                derivatives,
                request.InitialState,
                request.T0,
                request.TEnd,
                request.Tolerance,
                request.InitialStepSize);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SolveOdeRK45");
    }

    private static void MapDerivativeEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/derivative")
            .WithTags("Numerical Differentiation");

        group.MapPost("/gradient", async (GradientRequest request, DerivativeService service) =>
        {
            var result = service.Gradient(
                x => EvaluateQuadraticForm(request.QuadraticCoefficients, request.LinearCoefficients, request.Constant, x),
                request.Point,
                request.H);

            return result.IsSuccess
                ? Results.Ok(new { Gradient = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateGradient");

        group.MapPost("/hessian", async (GradientRequest request, DerivativeService service) =>
        {
            var result = service.Hessian(
                x => EvaluateQuadraticForm(request.QuadraticCoefficients, request.LinearCoefficients, request.Constant, x),
                request.Point,
                request.H);

            return result.IsSuccess
                ? Results.Ok(new { Hessian = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateHessian");
    }

    private static double[] EvaluateLinearSystem(double[][] coefficients, double[] constants, double t, double[] y)
    {
        // Linear system: dy[i]/dt = sum(A[i,j] * y[j]) + C[i]
        var n = y.Length;
        var result = new double[n];

        for (int i = 0; i < n; i++)
        {
            if (i < coefficients.Length)
            {
                for (int j = 0; j < Math.Min(n, coefficients[i].Length); j++)
                {
                    result[i] += coefficients[i][j] * y[j];
                }
            }
            if (i < constants.Length)
            {
                result[i] += constants[i];
            }
        }

        return result;
    }

    private static double EvaluateQuadraticForm(double[][] Q, double[] b, double c, double[] x)
    {
        // f(x) = 0.5 * x^T * Q * x + b^T * x + c
        var n = x.Length;
        double result = c;

        // Linear term
        for (int i = 0; i < n && i < b.Length; i++)
        {
            result += b[i] * x[i];
        }

        // Quadratic term
        for (int i = 0; i < n && i < Q.Length; i++)
        {
            for (int j = 0; j < n && j < Q[i].Length; j++)
            {
                result += 0.5 * Q[i][j] * x[i] * x[j];
            }
        }

        return result;
    }
}

public sealed class OdeSolveRequest
{
    public double[][] CoefficientMatrix { get; init; } = Array.Empty<double[]>();
    public double[] ConstantVector { get; init; } = Array.Empty<double>();
    public double[] InitialState { get; init; } = Array.Empty<double>();
    public double T0 { get; init; }
    public double TEnd { get; init; }
    public double StepSize { get; init; } = 0.01;
}

public sealed class OdeAdaptiveRequest
{
    public double[][] CoefficientMatrix { get; init; } = Array.Empty<double[]>();
    public double[] ConstantVector { get; init; } = Array.Empty<double>();
    public double[] InitialState { get; init; } = Array.Empty<double>();
    public double T0 { get; init; }
    public double TEnd { get; init; }
    public double Tolerance { get; init; } = 1e-6;
    public double InitialStepSize { get; init; } = 0.01;
}

public sealed class GradientRequest
{
    public double[][] QuadraticCoefficients { get; init; } = Array.Empty<double[]>();
    public double[] LinearCoefficients { get; init; } = Array.Empty<double>();
    public double Constant { get; init; }
    public double[] Point { get; init; } = Array.Empty<double>();
    public double H { get; init; } = 1e-8;
}
