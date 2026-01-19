namespace Optimization.Core.Entities;

public enum OptimizationAlgorithm
{
    SequentialQuadraticProgramming,
    GeneticAlgorithm,
    ParticleSwarm,
    DifferentialCorrection,
    GradientDescent,
    NelderMead
}

public enum OptimizationObjective
{
    MinimizeFuel,
    MinimizeTime,
    MinimizeDeltaV,
    MinimizeEnergy,
    Custom
}

public enum OptimizationStatus
{
    Queued,
    Running,
    Converged,
    MaxIterationsReached,
    Failed,
    Cancelled
}

public sealed class OptimizationJob
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? MissionId { get; private set; }

    // Algorithm configuration
    public OptimizationAlgorithm Algorithm { get; private set; }
    public OptimizationObjective Objective { get; private set; }
    public int MaxIterations { get; private set; }
    public double ConvergenceTolerance { get; private set; }

    // Status and progress
    public OptimizationStatus Status { get; private set; }
    public int CurrentIteration { get; private set; }
    public double CurrentCost { get; private set; }
    public double BestCost { get; private set; }
    public double? ConvergenceRate { get; private set; }

    // Design variables
    public List<DesignVariable> DesignVariables { get; private set; } = new();
    public List<OptimizationConstraint> Constraints { get; private set; } = new();

    // Solution
    public double[]? BestSolution { get; private set; }
    public double[]? FinalSolution { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long ComputationTimeMs { get; private set; }

    // Metadata
    public DateTime CreatedAt { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    private OptimizationJob() { }

    public static OptimizationJob Create(
        string name,
        OptimizationAlgorithm algorithm,
        OptimizationObjective objective,
        string createdByUserId,
        int maxIterations = 1000,
        double convergenceTolerance = 1e-8,
        string? description = null,
        Guid? missionId = null)
    {
        return new OptimizationJob
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            MissionId = missionId,
            Algorithm = algorithm,
            Objective = objective,
            MaxIterations = maxIterations,
            ConvergenceTolerance = convergenceTolerance,
            Status = OptimizationStatus.Queued,
            CurrentIteration = 0,
            CurrentCost = double.MaxValue,
            BestCost = double.MaxValue,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public void AddDesignVariable(string name, double initialValue, double lowerBound, double upperBound)
    {
        DesignVariables.Add(new DesignVariable(name, initialValue, lowerBound, upperBound));
    }

    public void AddConstraint(string name, ConstraintType type, double value, double tolerance = 0)
    {
        Constraints.Add(new OptimizationConstraint(name, type, value, tolerance));
    }

    public void Start()
    {
        if (Status != OptimizationStatus.Queued)
            throw new InvalidOperationException($"Cannot start job in {Status} status");

        Status = OptimizationStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(int iteration, double cost, double[]? currentSolution = null)
    {
        CurrentIteration = iteration;
        CurrentCost = cost;

        if (cost < BestCost)
        {
            BestCost = cost;
            BestSolution = currentSolution;
        }
    }

    public void Complete(double[] finalSolution, double finalCost, bool converged)
    {
        Status = converged ? OptimizationStatus.Converged : OptimizationStatus.MaxIterationsReached;
        FinalSolution = finalSolution;
        CurrentCost = finalCost;
        CompletedAt = DateTime.UtcNow;

        if (StartedAt.HasValue)
        {
            ComputationTimeMs = (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds;
        }
    }

    public void Fail(string reason)
    {
        Status = OptimizationStatus.Failed;
        Description = $"{Description}\nFailure reason: {reason}".Trim();
        CompletedAt = DateTime.UtcNow;

        if (StartedAt.HasValue)
        {
            ComputationTimeMs = (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds;
        }
    }

    public void Cancel()
    {
        if (Status == OptimizationStatus.Converged)
            throw new InvalidOperationException("Cannot cancel completed job");

        Status = OptimizationStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;

        if (StartedAt.HasValue)
        {
            ComputationTimeMs = (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds;
        }
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}

public sealed class DesignVariable
{
    public string Name { get; }
    public double InitialValue { get; }
    public double LowerBound { get; }
    public double UpperBound { get; }

    public DesignVariable(string name, double initialValue, double lowerBound, double upperBound)
    {
        Name = name;
        InitialValue = initialValue;
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }
}

public enum ConstraintType
{
    Equality,       // g(x) = value
    LessThan,       // g(x) <= value
    GreaterThan,    // g(x) >= value
    Bounded         // lower <= g(x) <= upper
}

public sealed class OptimizationConstraint
{
    public string Name { get; }
    public ConstraintType Type { get; }
    public double Value { get; }
    public double Tolerance { get; }

    public OptimizationConstraint(string name, ConstraintType type, double value, double tolerance = 0)
    {
        Name = name;
        Type = type;
        Value = value;
        Tolerance = tolerance;
    }

    public bool IsSatisfied(double evaluatedValue)
    {
        return Type switch
        {
            ConstraintType.Equality => Math.Abs(evaluatedValue - Value) <= Tolerance,
            ConstraintType.LessThan => evaluatedValue <= Value + Tolerance,
            ConstraintType.GreaterThan => evaluatedValue >= Value - Tolerance,
            _ => true
        };
    }
}
