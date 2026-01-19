using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for numerical differentiation.
/// </summary>
public sealed class DerivativeService
{
    public Result<double> Derivative(Func<double, double> function, double x, double h = 1e-8)
    {
        try
        {
            // Central difference: (f(x+h) - f(x-h)) / (2h)
            var result = (function(x + h) - function(x - h)) / (2 * h);
            return result;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Derivative calculation failed: {ex.Message}");
        }
    }

    public Result<double> ForwardDifference(Func<double, double> function, double x, double h = 1e-8)
    {
        try
        {
            // Forward difference: (f(x+h) - f(x)) / h
            var result = (function(x + h) - function(x)) / h;
            return result;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Forward difference failed: {ex.Message}");
        }
    }

    public Result<double> BackwardDifference(Func<double, double> function, double x, double h = 1e-8)
    {
        try
        {
            // Backward difference: (f(x) - f(x-h)) / h
            var result = (function(x) - function(x - h)) / h;
            return result;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Backward difference failed: {ex.Message}");
        }
    }

    public Result<double> SecondDerivative(Func<double, double> function, double x, double h = 1e-5)
    {
        try
        {
            // Second derivative: (f(x+h) - 2*f(x) + f(x-h)) / h^2
            var result = (function(x + h) - 2 * function(x) + function(x - h)) / (h * h);
            return result;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Second derivative calculation failed: {ex.Message}");
        }
    }

    public Result<double[]> Gradient(Func<double[], double> function, double[] x, double h = 1e-8)
    {
        try
        {
            var n = x.Length;
            var gradient = new double[n];

            for (int i = 0; i < n; i++)
            {
                var xPlus = (double[])x.Clone();
                var xMinus = (double[])x.Clone();
                xPlus[i] += h;
                xMinus[i] -= h;

                gradient[i] = (function(xPlus) - function(xMinus)) / (2 * h);
            }

            return gradient;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Gradient calculation failed: {ex.Message}");
        }
    }

    public Result<double[][]> Jacobian(Func<double[], double[]> function, double[] x, double h = 1e-8)
    {
        try
        {
            var n = x.Length;
            var f0 = function(x);
            var m = f0.Length;

            var jacobian = new double[m][];
            for (int i = 0; i < m; i++)
            {
                jacobian[i] = new double[n];
            }

            for (int j = 0; j < n; j++)
            {
                var xPlus = (double[])x.Clone();
                var xMinus = (double[])x.Clone();
                xPlus[j] += h;
                xMinus[j] -= h;

                var fPlus = function(xPlus);
                var fMinus = function(xMinus);

                for (int i = 0; i < m; i++)
                {
                    jacobian[i][j] = (fPlus[i] - fMinus[i]) / (2 * h);
                }
            }

            return jacobian;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Jacobian calculation failed: {ex.Message}");
        }
    }

    public Result<double[][]> Hessian(Func<double[], double> function, double[] x, double h = 1e-5)
    {
        try
        {
            var n = x.Length;
            var hessian = new double[n][];

            for (int i = 0; i < n; i++)
            {
                hessian[i] = new double[n];
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double d2f;

                    if (i == j)
                    {
                        // Diagonal element
                        var xPlus = (double[])x.Clone();
                        var xMinus = (double[])x.Clone();
                        xPlus[i] += h;
                        xMinus[i] -= h;

                        d2f = (function(xPlus) - 2 * function(x) + function(xMinus)) / (h * h);
                    }
                    else
                    {
                        // Off-diagonal element
                        var xPP = (double[])x.Clone();
                        var xPM = (double[])x.Clone();
                        var xMP = (double[])x.Clone();
                        var xMM = (double[])x.Clone();

                        xPP[i] += h; xPP[j] += h;
                        xPM[i] += h; xPM[j] -= h;
                        xMP[i] -= h; xMP[j] += h;
                        xMM[i] -= h; xMM[j] -= h;

                        d2f = (function(xPP) - function(xPM) - function(xMP) + function(xMM)) / (4 * h * h);
                    }

                    hessian[i][j] = d2f;
                    hessian[j][i] = d2f; // Symmetric
                }
            }

            return hessian;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Hessian calculation failed: {ex.Message}");
        }
    }
}
