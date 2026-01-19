namespace Optimization.Core.Entities;

/// <summary>
/// Delegate for objective function evaluation
/// </summary>
public delegate double ObjectiveFunction(double[] x);

/// <summary>
/// Delegate for gradient computation
/// </summary>
public delegate double[] GradientFunction(double[] x);

/// <summary>
/// Result of an optimization run
/// </summary>
public sealed class OptimizationResult
{
    public double[] Solution { get; init; } = Array.Empty<double>();
    public double ObjectiveValue { get; init; }
    public int Iterations { get; init; }
    public bool Converged { get; init; }
    public string TerminationReason { get; init; } = string.Empty;
    public List<double> CostHistory { get; init; } = new();
    public double[] GradientAtSolution { get; init; } = Array.Empty<double>();
}

/// <summary>
/// Interface for optimization algorithms
/// </summary>
public interface IOptimizer
{
    OptimizationAlgorithm Algorithm { get; }
    OptimizationResult Optimize(
        ObjectiveFunction objective,
        double[] initialGuess,
        double[] lowerBounds,
        double[] upperBounds,
        int maxIterations,
        double tolerance,
        Action<int, double, double[]>? progressCallback = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Gradient descent optimizer
/// </summary>
public sealed class GradientDescentOptimizer : IOptimizer
{
    private readonly double _learningRate;
    private readonly double _gradientEpsilon;

    public OptimizationAlgorithm Algorithm => OptimizationAlgorithm.GradientDescent;

    public GradientDescentOptimizer(double learningRate = 0.01, double gradientEpsilon = 1e-6)
    {
        _learningRate = learningRate;
        _gradientEpsilon = gradientEpsilon;
    }

    public OptimizationResult Optimize(
        ObjectiveFunction objective,
        double[] initialGuess,
        double[] lowerBounds,
        double[] upperBounds,
        int maxIterations,
        double tolerance,
        Action<int, double, double[]>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var x = (double[])initialGuess.Clone();
        var n = x.Length;
        var gradient = new double[n];
        var costHistory = new List<double>();
        var prevCost = double.MaxValue;
        var converged = false;
        var iterations = 0;
        var terminationReason = "Max iterations reached";

        for (var iter = 0; iter < maxIterations; iter++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            iterations = iter + 1;

            var cost = objective(x);
            costHistory.Add(cost);

            progressCallback?.Invoke(iter, cost, x);

            // Check convergence
            if (Math.Abs(prevCost - cost) < tolerance && iter > 0)
            {
                converged = true;
                terminationReason = "Converged: cost change below tolerance";
                break;
            }

            // Compute gradient using finite differences
            ComputeGradient(objective, x, gradient);

            // Check gradient norm
            var gradNorm = Math.Sqrt(gradient.Sum(g => g * g));
            if (gradNorm < tolerance)
            {
                converged = true;
                terminationReason = "Converged: gradient norm below tolerance";
                break;
            }

            // Update x
            for (var i = 0; i < n; i++)
            {
                x[i] -= _learningRate * gradient[i];
                x[i] = Math.Clamp(x[i], lowerBounds[i], upperBounds[i]);
            }

            prevCost = cost;
        }

        return new OptimizationResult
        {
            Solution = x,
            ObjectiveValue = objective(x),
            Iterations = iterations,
            Converged = converged,
            TerminationReason = terminationReason,
            CostHistory = costHistory,
            GradientAtSolution = gradient
        };
    }

    private void ComputeGradient(ObjectiveFunction objective, double[] x, double[] gradient)
    {
        var n = x.Length;
        var f0 = objective(x);

        for (var i = 0; i < n; i++)
        {
            var originalValue = x[i];
            x[i] = originalValue + _gradientEpsilon;
            var fp = objective(x);
            x[i] = originalValue - _gradientEpsilon;
            var fm = objective(x);
            gradient[i] = (fp - fm) / (2 * _gradientEpsilon);
            x[i] = originalValue;
        }
    }
}

/// <summary>
/// Nelder-Mead simplex optimizer (derivative-free)
/// </summary>
public sealed class NelderMeadOptimizer : IOptimizer
{
    private readonly double _alpha; // Reflection coefficient
    private readonly double _gamma; // Expansion coefficient
    private readonly double _rho;   // Contraction coefficient
    private readonly double _sigma; // Shrink coefficient

    public OptimizationAlgorithm Algorithm => OptimizationAlgorithm.NelderMead;

    public NelderMeadOptimizer(double alpha = 1.0, double gamma = 2.0, double rho = 0.5, double sigma = 0.5)
    {
        _alpha = alpha;
        _gamma = gamma;
        _rho = rho;
        _sigma = sigma;
    }

    public OptimizationResult Optimize(
        ObjectiveFunction objective,
        double[] initialGuess,
        double[] lowerBounds,
        double[] upperBounds,
        int maxIterations,
        double tolerance,
        Action<int, double, double[]>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var n = initialGuess.Length;
        var simplex = new double[n + 1][];
        var values = new double[n + 1];
        var costHistory = new List<double>();
        var converged = false;
        var iterations = 0;
        var terminationReason = "Max iterations reached";

        // Initialize simplex
        for (var i = 0; i <= n; i++)
        {
            simplex[i] = (double[])initialGuess.Clone();
            if (i > 0)
            {
                simplex[i][i - 1] += 0.1 * (upperBounds[i - 1] - lowerBounds[i - 1]);
                simplex[i][i - 1] = Math.Clamp(simplex[i][i - 1], lowerBounds[i - 1], upperBounds[i - 1]);
            }
            values[i] = objective(simplex[i]);
        }

        for (var iter = 0; iter < maxIterations; iter++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            iterations = iter + 1;

            // Sort vertices by objective value
            var indices = Enumerable.Range(0, n + 1).OrderBy(i => values[i]).ToArray();
            var orderedSimplex = indices.Select(i => simplex[i]).ToArray();
            var orderedValues = indices.Select(i => values[i]).ToArray();

            for (var i = 0; i <= n; i++)
            {
                simplex[i] = orderedSimplex[i];
                values[i] = orderedValues[i];
            }

            costHistory.Add(values[0]);
            progressCallback?.Invoke(iter, values[0], simplex[0]);

            // Check convergence
            var range = values[n] - values[0];
            if (range < tolerance)
            {
                converged = true;
                terminationReason = "Converged: simplex size below tolerance";
                break;
            }

            // Compute centroid of all but worst vertex
            var centroid = new double[n];
            for (var i = 0; i < n; i++)
            {
                centroid[i] = simplex.Take(n).Average(s => s[i]);
            }

            // Reflection
            var reflected = Reflect(centroid, simplex[n], _alpha, lowerBounds, upperBounds);
            var reflectedValue = objective(reflected);

            if (reflectedValue < values[0])
            {
                // Expansion
                var expanded = Reflect(centroid, simplex[n], _gamma, lowerBounds, upperBounds);
                var expandedValue = objective(expanded);

                if (expandedValue < reflectedValue)
                {
                    simplex[n] = expanded;
                    values[n] = expandedValue;
                }
                else
                {
                    simplex[n] = reflected;
                    values[n] = reflectedValue;
                }
            }
            else if (reflectedValue < values[n - 1])
            {
                simplex[n] = reflected;
                values[n] = reflectedValue;
            }
            else
            {
                // Contraction
                var contracted = Reflect(centroid, simplex[n], -_rho, lowerBounds, upperBounds);
                var contractedValue = objective(contracted);

                if (contractedValue < values[n])
                {
                    simplex[n] = contracted;
                    values[n] = contractedValue;
                }
                else
                {
                    // Shrink
                    for (var i = 1; i <= n; i++)
                    {
                        for (var j = 0; j < n; j++)
                        {
                            simplex[i][j] = simplex[0][j] + _sigma * (simplex[i][j] - simplex[0][j]);
                            simplex[i][j] = Math.Clamp(simplex[i][j], lowerBounds[j], upperBounds[j]);
                        }
                        values[i] = objective(simplex[i]);
                    }
                }
            }
        }

        var bestIdx = Array.IndexOf(values, values.Min());
        return new OptimizationResult
        {
            Solution = simplex[bestIdx],
            ObjectiveValue = values[bestIdx],
            Iterations = iterations,
            Converged = converged,
            TerminationReason = terminationReason,
            CostHistory = costHistory,
            GradientAtSolution = Array.Empty<double>()
        };
    }

    private static double[] Reflect(double[] centroid, double[] worst, double coefficient, double[] lowerBounds, double[] upperBounds)
    {
        var n = centroid.Length;
        var reflected = new double[n];
        for (var i = 0; i < n; i++)
        {
            reflected[i] = centroid[i] + coefficient * (centroid[i] - worst[i]);
            reflected[i] = Math.Clamp(reflected[i], lowerBounds[i], upperBounds[i]);
        }
        return reflected;
    }
}

/// <summary>
/// Particle Swarm Optimizer
/// </summary>
public sealed class ParticleSwarmOptimizer : IOptimizer
{
    private readonly int _swarmSize;
    private readonly double _w;     // Inertia weight
    private readonly double _c1;    // Cognitive coefficient
    private readonly double _c2;    // Social coefficient

    public OptimizationAlgorithm Algorithm => OptimizationAlgorithm.ParticleSwarm;

    public ParticleSwarmOptimizer(int swarmSize = 30, double inertiaWeight = 0.7, double cognitiveCoefficient = 1.5, double socialCoefficient = 1.5)
    {
        _swarmSize = swarmSize;
        _w = inertiaWeight;
        _c1 = cognitiveCoefficient;
        _c2 = socialCoefficient;
    }

    public OptimizationResult Optimize(
        ObjectiveFunction objective,
        double[] initialGuess,
        double[] lowerBounds,
        double[] upperBounds,
        int maxIterations,
        double tolerance,
        Action<int, double, double[]>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var n = initialGuess.Length;
        var random = new Random();
        var costHistory = new List<double>();
        var converged = false;
        var iterations = 0;
        var terminationReason = "Max iterations reached";

        // Initialize particles
        var positions = new double[_swarmSize][];
        var velocities = new double[_swarmSize][];
        var personalBest = new double[_swarmSize][];
        var personalBestValues = new double[_swarmSize];
        var globalBest = (double[])initialGuess.Clone();
        var globalBestValue = double.MaxValue;

        for (var i = 0; i < _swarmSize; i++)
        {
            positions[i] = new double[n];
            velocities[i] = new double[n];
            personalBest[i] = new double[n];

            for (var j = 0; j < n; j++)
            {
                if (i == 0)
                {
                    positions[i][j] = initialGuess[j];
                }
                else
                {
                    positions[i][j] = lowerBounds[j] + random.NextDouble() * (upperBounds[j] - lowerBounds[j]);
                }
                velocities[i][j] = 0.1 * (random.NextDouble() - 0.5) * (upperBounds[j] - lowerBounds[j]);
                personalBest[i][j] = positions[i][j];
            }

            personalBestValues[i] = objective(positions[i]);
            if (personalBestValues[i] < globalBestValue)
            {
                globalBestValue = personalBestValues[i];
                globalBest = (double[])positions[i].Clone();
            }
        }

        var prevBest = globalBestValue;

        for (var iter = 0; iter < maxIterations; iter++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            iterations = iter + 1;

            // Update particles
            for (var i = 0; i < _swarmSize; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    var r1 = random.NextDouble();
                    var r2 = random.NextDouble();

                    velocities[i][j] = _w * velocities[i][j]
                        + _c1 * r1 * (personalBest[i][j] - positions[i][j])
                        + _c2 * r2 * (globalBest[j] - positions[i][j]);

                    positions[i][j] += velocities[i][j];
                    positions[i][j] = Math.Clamp(positions[i][j], lowerBounds[j], upperBounds[j]);
                }

                var value = objective(positions[i]);
                if (value < personalBestValues[i])
                {
                    personalBestValues[i] = value;
                    personalBest[i] = (double[])positions[i].Clone();

                    if (value < globalBestValue)
                    {
                        globalBestValue = value;
                        globalBest = (double[])positions[i].Clone();
                    }
                }
            }

            costHistory.Add(globalBestValue);
            progressCallback?.Invoke(iter, globalBestValue, globalBest);

            // Check convergence
            if (Math.Abs(prevBest - globalBestValue) < tolerance && iter > 10)
            {
                converged = true;
                terminationReason = "Converged: improvement below tolerance";
                break;
            }

            prevBest = globalBestValue;
        }

        return new OptimizationResult
        {
            Solution = globalBest,
            ObjectiveValue = globalBestValue,
            Iterations = iterations,
            Converged = converged,
            TerminationReason = terminationReason,
            CostHistory = costHistory,
            GradientAtSolution = Array.Empty<double>()
        };
    }
}

/// <summary>
/// Factory for creating optimizers
/// </summary>
public static class OptimizerFactory
{
    public static IOptimizer Create(OptimizationAlgorithm algorithm)
    {
        return algorithm switch
        {
            OptimizationAlgorithm.GradientDescent => new GradientDescentOptimizer(),
            OptimizationAlgorithm.NelderMead => new NelderMeadOptimizer(),
            OptimizationAlgorithm.ParticleSwarm => new ParticleSwarmOptimizer(),
            _ => throw new NotSupportedException($"Algorithm {algorithm} is not yet implemented")
        };
    }
}
