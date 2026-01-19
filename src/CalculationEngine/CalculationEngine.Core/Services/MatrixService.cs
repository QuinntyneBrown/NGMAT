using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for matrix operations.
/// </summary>
public sealed class MatrixService
{
    public Result<double[][]> Add(double[][] a, double[][] b)
    {
        try
        {
            var matrixA = CreateMatrix(a);
            var matrixB = CreateMatrix(b);
            var result = matrixA + matrixB;
            return ToJagged(result);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Matrix addition failed: {ex.Message}");
        }
    }

    public Result<double[][]> Subtract(double[][] a, double[][] b)
    {
        try
        {
            var matrixA = CreateMatrix(a);
            var matrixB = CreateMatrix(b);
            var result = matrixA - matrixB;
            return ToJagged(result);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Matrix subtraction failed: {ex.Message}");
        }
    }

    public Result<double[][]> Multiply(double[][] a, double[][] b)
    {
        try
        {
            var matrixA = CreateMatrix(a);
            var matrixB = CreateMatrix(b);
            var result = matrixA * matrixB;
            return ToJagged(result);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Matrix multiplication failed: {ex.Message}");
        }
    }

    public Result<double[][]> Transpose(double[][] a)
    {
        try
        {
            var matrix = CreateMatrix(a);
            var result = matrix.Transpose();
            return ToJagged(result);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Matrix transpose failed: {ex.Message}");
        }
    }

    public Result<double[][]> Inverse(double[][] a)
    {
        try
        {
            var matrix = CreateMatrix(a);
            var result = matrix.Inverse();
            return ToJagged(result);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Matrix inverse failed: {ex.Message}");
        }
    }

    public Result<double> Determinant(double[][] a)
    {
        try
        {
            var matrix = CreateMatrix(a);
            return matrix.Determinant();
        }
        catch (Exception ex)
        {
            return Error.Validation($"Determinant calculation failed: {ex.Message}");
        }
    }

    public Result<EigenResult> Eigenvalues(double[][] a)
    {
        try
        {
            var matrix = CreateMatrix(a);
            var evd = matrix.Evd();

            return new EigenResult
            {
                EigenvaluesReal = evd.EigenValues.Select(c => c.Real).ToArray(),
                EigenvaluesImaginary = evd.EigenValues.Select(c => c.Imaginary).ToArray(),
                Eigenvectors = ToJagged(evd.EigenVectors)
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Eigenvalue computation failed: {ex.Message}");
        }
    }

    public Result<LuDecompositionResult> LuDecomposition(double[][] a)
    {
        try
        {
            var matrix = CreateMatrix(a);
            var lu = matrix.LU();

            // Extract permutation indices manually
            var permutation = lu.P;
            var pArray = new int[permutation.Dimension];
            for (int i = 0; i < permutation.Dimension; i++)
            {
                pArray[i] = permutation[i];
            }

            return new LuDecompositionResult
            {
                L = ToJagged(lu.L),
                U = ToJagged(lu.U),
                P = pArray
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"LU decomposition failed: {ex.Message}");
        }
    }

    public Result<double[]> Solve(double[][] a, double[] b)
    {
        try
        {
            var matrix = CreateMatrix(a);
            var vector = Vector<double>.Build.DenseOfArray(b);
            var result = matrix.Solve(vector);
            return result.ToArray();
        }
        catch (Exception ex)
        {
            return Error.Validation($"Linear system solving failed: {ex.Message}");
        }
    }

    private static Matrix<double> CreateMatrix(double[][] jagged)
    {
        return DenseMatrix.OfRowArrays(jagged);
    }

    private static double[][] ToJagged(Matrix<double> matrix)
    {
        var result = new double[matrix.RowCount][];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            result[i] = matrix.Row(i).ToArray();
        }
        return result;
    }
}

public sealed class EigenResult
{
    public double[] EigenvaluesReal { get; init; } = Array.Empty<double>();
    public double[] EigenvaluesImaginary { get; init; } = Array.Empty<double>();
    public double[][] Eigenvectors { get; init; } = Array.Empty<double[]>();
}

public sealed class LuDecompositionResult
{
    public double[][] L { get; init; } = Array.Empty<double[]>();
    public double[][] U { get; init; } = Array.Empty<double[]>();
    public int[] P { get; init; } = Array.Empty<int>();
}
