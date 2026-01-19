using MathNet.Numerics.OdeSolvers;
using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for solving ordinary differential equations.
/// </summary>
public sealed class OdeSolverService
{
    public Result<OdeSolutionResult> SolveRk4(
        Func<double, double[], double[]> derivatives,
        double[] initialState,
        double t0,
        double tEnd,
        double stepSize)
    {
        try
        {
            var times = new List<double>();
            var states = new List<double[]>();

            var t = t0;
            var state = (double[])initialState.Clone();

            times.Add(t);
            states.Add((double[])state.Clone());

            while (t < tEnd)
            {
                var h = Math.Min(stepSize, tEnd - t);
                state = RungeKutta4Step(derivatives, t, state, h);
                t += h;

                times.Add(t);
                states.Add((double[])state.Clone());
            }

            return new OdeSolutionResult
            {
                Times = times.ToArray(),
                States = states.ToArray(),
                Method = "RK4"
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"RK4 integration failed: {ex.Message}");
        }
    }

    public Result<OdeSolutionResult> SolveRk45(
        Func<double, double[], double[]> derivatives,
        double[] initialState,
        double t0,
        double tEnd,
        double tolerance = 1e-6,
        double initialStepSize = 0.01)
    {
        try
        {
            var times = new List<double>();
            var states = new List<double[]>();

            var t = t0;
            var state = (double[])initialState.Clone();
            var h = initialStepSize;
            var minStep = 1e-10;
            var maxStep = (tEnd - t0) / 10;

            times.Add(t);
            states.Add((double[])state.Clone());

            while (t < tEnd)
            {
                h = Math.Min(h, tEnd - t);

                var (newState, error) = RungeKutta45Step(derivatives, t, state, h);

                if (error < tolerance || h <= minStep)
                {
                    t += h;
                    state = newState;
                    times.Add(t);
                    states.Add((double[])state.Clone());

                    // Increase step size
                    if (error > 0)
                    {
                        h *= Math.Min(2.0, 0.84 * Math.Pow(tolerance / error, 0.25));
                    }
                    h = Math.Min(h, maxStep);
                }
                else
                {
                    // Reduce step size
                    h *= Math.Max(0.1, 0.84 * Math.Pow(tolerance / error, 0.25));
                }

                h = Math.Max(h, minStep);
            }

            return new OdeSolutionResult
            {
                Times = times.ToArray(),
                States = states.ToArray(),
                Method = "RK45"
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"RK45 integration failed: {ex.Message}");
        }
    }

    private static double[] RungeKutta4Step(
        Func<double, double[], double[]> f,
        double t,
        double[] y,
        double h)
    {
        var n = y.Length;

        var k1 = f(t, y);
        var k2 = f(t + h / 2, AddVectors(y, ScaleVector(k1, h / 2)));
        var k3 = f(t + h / 2, AddVectors(y, ScaleVector(k2, h / 2)));
        var k4 = f(t + h, AddVectors(y, ScaleVector(k3, h)));

        var result = new double[n];
        for (int i = 0; i < n; i++)
        {
            result[i] = y[i] + (h / 6) * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
        }

        return result;
    }

    private static (double[] state, double error) RungeKutta45Step(
        Func<double, double[], double[]> f,
        double t,
        double[] y,
        double h)
    {
        // Dormand-Prince coefficients
        var n = y.Length;

        var k1 = f(t, y);
        var k2 = f(t + h / 5, AddVectors(y, ScaleVector(k1, h / 5)));
        var k3 = f(t + 3 * h / 10, AddVectors(y, AddVectors(ScaleVector(k1, 3 * h / 40), ScaleVector(k2, 9 * h / 40))));
        var k4 = f(t + 4 * h / 5, AddVectors(y, AddVectors(AddVectors(ScaleVector(k1, 44 * h / 45), ScaleVector(k2, -56 * h / 15)), ScaleVector(k3, 32 * h / 9))));
        var k5 = f(t + 8 * h / 9, AddVectors(y, AddVectors(AddVectors(AddVectors(ScaleVector(k1, 19372 * h / 6561), ScaleVector(k2, -25360 * h / 2187)), ScaleVector(k3, 64448 * h / 6561)), ScaleVector(k4, -212 * h / 729))));
        var k6 = f(t + h, AddVectors(y, AddVectors(AddVectors(AddVectors(AddVectors(ScaleVector(k1, 9017 * h / 3168), ScaleVector(k2, -355 * h / 33)), ScaleVector(k3, 46732 * h / 5247)), ScaleVector(k4, 49 * h / 176)), ScaleVector(k5, -5103 * h / 18656))));

        // 5th order solution
        var y5 = new double[n];
        for (int i = 0; i < n; i++)
        {
            y5[i] = y[i] + h * (35 * k1[i] / 384 + 500 * k3[i] / 1113 + 125 * k4[i] / 192 - 2187 * k5[i] / 6784 + 11 * k6[i] / 84);
        }

        // 4th order solution for error estimation
        var k7 = f(t + h, y5);
        var y4 = new double[n];
        for (int i = 0; i < n; i++)
        {
            y4[i] = y[i] + h * (5179 * k1[i] / 57600 + 7571 * k3[i] / 16695 + 393 * k4[i] / 640 - 92097 * k5[i] / 339200 + 187 * k6[i] / 2100 + k7[i] / 40);
        }

        // Error estimate
        var error = 0.0;
        for (int i = 0; i < n; i++)
        {
            error = Math.Max(error, Math.Abs(y5[i] - y4[i]));
        }

        return (y5, error);
    }

    private static double[] AddVectors(double[] a, double[] b)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }
        return result;
    }

    private static double[] ScaleVector(double[] v, double s)
    {
        var result = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            result[i] = v[i] * s;
        }
        return result;
    }
}

public sealed class OdeSolutionResult
{
    public double[] Times { get; init; } = Array.Empty<double>();
    public double[][] States { get; init; } = Array.Empty<double[]>();
    public string Method { get; init; } = string.Empty;
}
