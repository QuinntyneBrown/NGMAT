using MathNet.Numerics.Interpolation;
using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for interpolation operations.
/// </summary>
public sealed class InterpolationService
{
    public Result<double> LinearInterpolate(double[] x, double[] y, double xi)
    {
        try
        {
            var interpolation = LinearSpline.Interpolate(x, y);
            return interpolation.Interpolate(xi);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Linear interpolation failed: {ex.Message}");
        }
    }

    public Result<double[]> LinearInterpolateBatch(double[] x, double[] y, double[] xi)
    {
        try
        {
            var interpolation = LinearSpline.Interpolate(x, y);
            var results = xi.Select(v => interpolation.Interpolate(v)).ToArray();
            return results;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Linear interpolation failed: {ex.Message}");
        }
    }

    public Result<double> CubicSplineInterpolate(double[] x, double[] y, double xi)
    {
        try
        {
            var interpolation = CubicSpline.InterpolateNatural(x, y);
            return interpolation.Interpolate(xi);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Cubic spline interpolation failed: {ex.Message}");
        }
    }

    public Result<double[]> CubicSplineInterpolateBatch(double[] x, double[] y, double[] xi)
    {
        try
        {
            var interpolation = CubicSpline.InterpolateNatural(x, y);
            var results = xi.Select(v => interpolation.Interpolate(v)).ToArray();
            return results;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Cubic spline interpolation failed: {ex.Message}");
        }
    }

    public Result<double> HermiteInterpolate(double[] x, double[] y, double[] dy, double xi)
    {
        try
        {
            var interpolation = CubicSpline.InterpolateHermite(x, y, dy);
            return interpolation.Interpolate(xi);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Hermite interpolation failed: {ex.Message}");
        }
    }

    public Result<double[]> HermiteInterpolateBatch(double[] x, double[] y, double[] dy, double[] xi)
    {
        try
        {
            var interpolation = CubicSpline.InterpolateHermite(x, y, dy);
            var results = xi.Select(v => interpolation.Interpolate(v)).ToArray();
            return results;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Hermite interpolation failed: {ex.Message}");
        }
    }

    public Result<double> PolynomialInterpolate(double[] x, double[] y, double xi)
    {
        try
        {
            var interpolation = Barycentric.InterpolatePolynomialEquidistant(x, y);
            return interpolation.Interpolate(xi);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Polynomial interpolation failed: {ex.Message}");
        }
    }

    public Result<double> AkimaInterpolate(double[] x, double[] y, double xi)
    {
        try
        {
            var interpolation = CubicSpline.InterpolateAkima(x, y);
            return interpolation.Interpolate(xi);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Akima interpolation failed: {ex.Message}");
        }
    }
}
