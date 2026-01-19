using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for root finding operations.
/// </summary>
public sealed class RootFindingService
{
    public Result<double> FindRootNewtonRaphson(
        Func<double, double> function,
        Func<double, double> derivative,
        double initialGuess,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        try
        {
            var root = MathNet.Numerics.RootFinding.NewtonRaphson.FindRoot(function, derivative, initialGuess, tolerance, maxIterations);
            return root;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Newton-Raphson failed to converge: {ex.Message}");
        }
    }

    public Result<double> FindRootBisection(
        Func<double, double> function,
        double lowerBound,
        double upperBound,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        try
        {
            var root = MathNet.Numerics.RootFinding.Bisection.FindRoot(function, lowerBound, upperBound, tolerance, maxIterations);
            return root;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Bisection method failed: {ex.Message}");
        }
    }

    public Result<double> FindRootBrent(
        Func<double, double> function,
        double lowerBound,
        double upperBound,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        try
        {
            var root = MathNet.Numerics.RootFinding.Brent.FindRoot(function, lowerBound, upperBound, tolerance, maxIterations);
            return root;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Brent's method failed: {ex.Message}");
        }
    }

    public Result<double> FindRootSecant(
        Func<double, double> function,
        double guess1,
        double guess2,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        try
        {
            var root = MathNet.Numerics.RootFinding.Secant.FindRoot(function, guess1, guess2, tolerance, maxIterations);
            return root;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Secant method failed: {ex.Message}");
        }
    }

    public Result<double[]> MultiDimensional(
        Func<double[], double[]> functions,
        double[] initialGuess,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        try
        {
            // Simple Newton-Raphson for multi-dimensional case
            var x = (double[])initialGuess.Clone();
            var n = x.Length;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                var f = functions(x);

                // Check convergence
                var maxError = f.Max(Math.Abs);
                if (maxError < tolerance)
                {
                    return x;
                }

                // Compute Jacobian numerically
                var jacobian = ComputeJacobian(functions, x, 1e-8);

                // Solve Jacobian * delta = -f
                var delta = SolveLinearSystem(jacobian, f.Select(v => -v).ToArray());

                // Update x
                for (int i = 0; i < n; i++)
                {
                    x[i] += delta[i];
                }
            }

            return Error.Validation("Multi-dimensional root finding did not converge");
        }
        catch (Exception ex)
        {
            return Error.Validation($"Multi-dimensional root finding failed: {ex.Message}");
        }
    }

    private static double[][] ComputeJacobian(Func<double[], double[]> f, double[] x, double h)
    {
        var n = x.Length;
        var f0 = f(x);
        var m = f0.Length;

        var jacobian = new double[m][];
        for (int i = 0; i < m; i++)
        {
            jacobian[i] = new double[n];
        }

        for (int j = 0; j < n; j++)
        {
            var xPlus = (double[])x.Clone();
            xPlus[j] += h;
            var fPlus = f(xPlus);

            for (int i = 0; i < m; i++)
            {
                jacobian[i][j] = (fPlus[i] - f0[i]) / h;
            }
        }

        return jacobian;
    }

    private static double[] SolveLinearSystem(double[][] a, double[] b)
    {
        var matrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfRowArrays(a);
        var vector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(b);
        var result = matrix.Solve(vector);
        return result.ToArray();
    }
}
