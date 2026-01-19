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
            // For API, we can only expose simple polynomial-like functions
            // A real implementation would need expression parsing
            var result = service.SolveRK4(
                (t, y) => EvaluateSimpleDerivative(request.DerivativeCoefficients, t, y),
                request.Y0,
                request.T0,
                request.Tf,
                request.Steps);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SolveOdeRK4");

        group.MapPost("/rk45", async (OdeAdaptiveRequest request, OdeSolverService service) =>
        {
            var result = service.SolveRK45(
                (t, y) => EvaluateSimpleDerivative(request.DerivativeCoefficients, t, y),
                request.Y0,
                request.T0,
                request.Tf,
                request.Tolerance);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SolveOdeRK45");

        group.MapPost("/system/rk4", async (OdeSystemRequest request, OdeSolverService service) =>
        {
            var result = service.SolveSystemRK4(
                (t, y) => EvaluateSystemDerivative(request.SystemCoefficients, t, y),
                request.Y0,
                request.T0,
                request.Tf,
                request.Steps);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SolveOdeSystemRK4");
    }

    private static void MapDerivativeEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/derivative")
            .WithTags("Numerical Differentiation");

        group.MapPost("/gradient", async (GradientRequest request, DerivativeService service) =>
        {
            var result = service.Gradient(
                x => EvaluatePolynomial(request.Coefficients, x),
                request.Point,
                request.H);

            return result.IsSuccess
                ? Results.Ok(new { Gradient = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateGradient");

        group.MapPost("/hessian", async (GradientRequest request, DerivativeService service) =>
        {
            var result = service.Hessian(
                x => EvaluatePolynomial(request.Coefficients, x),
                request.Point,
                request.H);

            return result.IsSuccess
                ? Results.Ok(new { Hessian = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("CalculateHessian");
    }

    private static double EvaluateSimpleDerivative(double[] coefficients, double t, double y)
    {
        // Simple linear derivative: dy/dt = a*y + b*t + c
        // coefficients[0] = a, coefficients[1] = b, coefficients[2] = c
        if (coefficients.Length < 3) return 0;
        return coefficients[0] * y + coefficients[1] * t + coefficients[2];
    }

    private static double[] EvaluateSystemDerivative(double[][] systemCoefficients, double t, double[] y)
    {
        // Linear system: dy[i]/dt = sum(A[i,j] * y[j]) + B[i] * t + C[i]
        var n = y.Length;
        var result = new double[n];

        for (int i = 0; i < n; i++)
        {
            if (i < systemCoefficients.Length && systemCoefficients[i].Length >= n + 2)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i] += systemCoefficients[i][j] * y[j];
                }
                result[i] += systemCoefficients[i][n] * t + systemCoefficients[i][n + 1];
            }
        }

        return result;
    }

    private static double EvaluatePolynomial(double[][] coefficients, double[] x)
    {
        // Simple quadratic: f(x) = sum(a[i] * x[i]^2) + sum(b[i] * x[i]) + c
        double result = 0;
        for (int i = 0; i < x.Length && i < coefficients.Length; i++)
        {
            if (coefficients[i].Length >= 2)
            {
                result += coefficients[i][0] * x[i] * x[i] + coefficients[i][1] * x[i];
            }
            if (coefficients[i].Length >= 3)
            {
                result += coefficients[i][2];
            }
        }
        return result;
    }
}

public sealed class OdeSolveRequest
{
    public double[] DerivativeCoefficients { get; init; } = Array.Empty<double>();
    public double Y0 { get; init; }
    public double T0 { get; init; }
    public double Tf { get; init; }
    public int Steps { get; init; } = 100;
}

public sealed class OdeAdaptiveRequest
{
    public double[] DerivativeCoefficients { get; init; } = Array.Empty<double>();
    public double Y0 { get; init; }
    public double T0 { get; init; }
    public double Tf { get; init; }
    public double Tolerance { get; init; } = 1e-6;
}

public sealed class OdeSystemRequest
{
    public double[][] SystemCoefficients { get; init; } = Array.Empty<double[]>();
    public double[] Y0 { get; init; } = Array.Empty<double>();
    public double T0 { get; init; }
    public double Tf { get; init; }
    public int Steps { get; init; } = 100;
}

public sealed class GradientRequest
{
    public double[][] Coefficients { get; init; } = Array.Empty<double[]>();
    public double[] Point { get; init; } = Array.Empty<double>();
    public double H { get; init; } = 1e-8;
}
