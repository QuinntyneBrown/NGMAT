namespace Propagation.Core.Entities;

public enum IntegratorType
{
    RungeKutta4,       // Fixed step RK4
    RungeKutta45,      // Adaptive Dormand-Prince 5(4)
    RungeKutta78,      // Adaptive Fehlberg 7(8)
    AdamsBashforth,    // Multi-step method
    GaussJackson       // Predictor-corrector
}

public enum OutputMode
{
    FixedStep,         // Output at regular intervals
    IntegrationStep,   // Output at every integration step
    StartAndEnd,       // Only output start and end states
    Custom             // Custom output epochs
}

public sealed class PropagationConfiguration
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? MissionId { get; private set; }

    // Integrator settings
    public IntegratorType Integrator { get; private set; } = IntegratorType.RungeKutta45;
    public double InitialStepSizeSeconds { get; private set; } = 60.0;
    public double MinStepSizeSeconds { get; private set; } = 1.0;
    public double MaxStepSizeSeconds { get; private set; } = 3600.0;

    // Error control for adaptive integrators
    public double RelativeTolerance { get; private set; } = 1e-10;
    public double AbsoluteTolerance { get; private set; } = 1e-10;

    // Output settings
    public OutputMode OutputMode { get; private set; } = OutputMode.FixedStep;
    public double OutputStepSizeSeconds { get; private set; } = 60.0;

    // Force model reference
    public Guid? ForceModelConfigurationId { get; private set; }

    // Stopping conditions
    public double? MaxPropagationDurationSeconds { get; private set; }
    public double? MaxStepCount { get; private set; }
    public double? MinAltitudeMeters { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    private PropagationConfiguration() { }

    public static PropagationConfiguration Create(
        string name,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null)
    {
        return new PropagationConfiguration
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            MissionId = missionId,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public void SetIntegratorSettings(
        IntegratorType integrator,
        double initialStepSize,
        double minStepSize,
        double maxStepSize,
        double relativeTolerance,
        double absoluteTolerance)
    {
        Integrator = integrator;
        InitialStepSizeSeconds = initialStepSize;
        MinStepSizeSeconds = minStepSize;
        MaxStepSizeSeconds = maxStepSize;
        RelativeTolerance = relativeTolerance;
        AbsoluteTolerance = absoluteTolerance;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOutputSettings(OutputMode mode, double stepSizeSeconds)
    {
        OutputMode = mode;
        OutputStepSizeSeconds = stepSizeSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetForceModelReference(Guid? forceModelConfigurationId)
    {
        ForceModelConfigurationId = forceModelConfigurationId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStoppingConditions(
        double? maxDurationSeconds,
        double? maxStepCount,
        double? minAltitudeMeters)
    {
        MaxPropagationDurationSeconds = maxDurationSeconds;
        MaxStepCount = maxStepCount;
        MinAltitudeMeters = minAltitudeMeters;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public static class StandardPropagationConfigs
{
    public static PropagationConfiguration CreateFastPropagation(string userId)
    {
        var config = PropagationConfiguration.Create("Fast Propagation", userId, "Quick propagation with RK4");
        config.SetIntegratorSettings(IntegratorType.RungeKutta4, 60, 1, 120, 1e-8, 1e-8);
        config.SetOutputSettings(OutputMode.FixedStep, 60);
        return config;
    }

    public static PropagationConfiguration CreatePrecisePropagation(string userId)
    {
        var config = PropagationConfiguration.Create("Precise Propagation", userId, "High accuracy with adaptive step size");
        config.SetIntegratorSettings(IntegratorType.RungeKutta45, 60, 0.1, 600, 1e-12, 1e-12);
        config.SetOutputSettings(OutputMode.FixedStep, 60);
        return config;
    }

    public static PropagationConfiguration CreateLongTermPropagation(string userId)
    {
        var config = PropagationConfiguration.Create("Long Term Propagation", userId, "Optimized for long duration propagations");
        config.SetIntegratorSettings(IntegratorType.RungeKutta78, 300, 1, 3600, 1e-10, 1e-10);
        config.SetOutputSettings(OutputMode.FixedStep, 600);
        return config;
    }
}
