using MathNet.Numerics.Statistics;
using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for statistical computations.
/// </summary>
public sealed class StatisticsService
{
    public Result<double> Mean(double[] values)
    {
        try
        {
            return Statistics.Mean(values);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Mean calculation failed: {ex.Message}");
        }
    }

    public Result<double> Median(double[] values)
    {
        try
        {
            return Statistics.Median(values);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Median calculation failed: {ex.Message}");
        }
    }

    public Result<double> StandardDeviation(double[] values)
    {
        try
        {
            return Statistics.StandardDeviation(values);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Standard deviation calculation failed: {ex.Message}");
        }
    }

    public Result<double> Variance(double[] values)
    {
        try
        {
            return Statistics.Variance(values);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Variance calculation failed: {ex.Message}");
        }
    }

    public Result<double> Min(double[] values)
    {
        try
        {
            return Statistics.Minimum(values);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Min calculation failed: {ex.Message}");
        }
    }

    public Result<double> Max(double[] values)
    {
        try
        {
            return Statistics.Maximum(values);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Max calculation failed: {ex.Message}");
        }
    }

    public Result<DescriptiveStatisticsResult> DescriptiveStatistics(double[] values)
    {
        try
        {
            var stats = new DescriptiveStatistics(values);
            return new DescriptiveStatisticsResult
            {
                Count = stats.Count,
                Mean = stats.Mean,
                StandardDeviation = stats.StandardDeviation,
                Variance = stats.Variance,
                Min = stats.Minimum,
                Max = stats.Maximum,
                Skewness = stats.Skewness,
                Kurtosis = stats.Kurtosis
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Descriptive statistics calculation failed: {ex.Message}");
        }
    }

    public Result<double[]> Percentiles(double[] values, double[] percentiles)
    {
        try
        {
            var sorted = values.OrderBy(v => v).ToArray();
            var results = new double[percentiles.Length];

            for (int i = 0; i < percentiles.Length; i++)
            {
                results[i] = Statistics.Percentile(sorted, (int)percentiles[i]);
            }

            return results;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Percentile calculation failed: {ex.Message}");
        }
    }

    public Result<double> Covariance(double[] x, double[] y)
    {
        try
        {
            return Statistics.Covariance(x, y);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Covariance calculation failed: {ex.Message}");
        }
    }

    public Result<double> PearsonCorrelation(double[] x, double[] y)
    {
        try
        {
            return MathNet.Numerics.Statistics.Correlation.Pearson(x, y);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Correlation calculation failed: {ex.Message}");
        }
    }

    public Result<double[][]> CovarianceMatrix(double[][] data)
    {
        try
        {
            var n = data.Length;
            if (n == 0) return Array.Empty<double[]>();

            var m = data[0].Length;
            var matrix = new double[m][];

            for (int i = 0; i < m; i++)
            {
                matrix[i] = new double[m];
                for (int j = 0; j < m; j++)
                {
                    var colI = data.Select(row => row[i]).ToArray();
                    var colJ = data.Select(row => row[j]).ToArray();
                    matrix[i][j] = Statistics.Covariance(colI, colJ);
                }
            }

            return matrix;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Covariance matrix calculation failed: {ex.Message}");
        }
    }

    public Result<HistogramResult> Histogram(double[] values, int buckets = 10)
    {
        try
        {
            var histogram = new Histogram(values, buckets);
            var bucketBoundaries = new double[buckets + 1];
            var counts = new int[buckets];

            bucketBoundaries[0] = histogram.LowerBound;
            for (int i = 0; i < buckets; i++)
            {
                var bucket = histogram[i];
                bucketBoundaries[i + 1] = bucket.UpperBound;
                counts[i] = (int)bucket.Count;
            }

            return new HistogramResult
            {
                BucketBoundaries = bucketBoundaries,
                Counts = counts
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Histogram generation failed: {ex.Message}");
        }
    }
}

public sealed class DescriptiveStatisticsResult
{
    public long Count { get; init; }
    public double Mean { get; init; }
    public double StandardDeviation { get; init; }
    public double Variance { get; init; }
    public double Min { get; init; }
    public double Max { get; init; }
    public double Skewness { get; init; }
    public double Kurtosis { get; init; }
}

public sealed class HistogramResult
{
    public double[] BucketBoundaries { get; init; } = Array.Empty<double>();
    public int[] Counts { get; init; } = Array.Empty<int>();
}
