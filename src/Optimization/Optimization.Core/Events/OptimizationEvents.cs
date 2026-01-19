using MessagePack;
using Shared.Messaging.Abstractions;

namespace Optimization.Core.Events;

[MessagePackObject]
public sealed class OptimizationStartedEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string Algorithm { get; init; } = string.Empty;

    [Key(13)]
    public string Objective { get; init; } = string.Empty;

    [Key(14)]
    public int MaxIterations { get; init; }

    public OptimizationStartedEvent() : base()
    {
        SourceService = "Optimization";
    }
}

[MessagePackObject]
public sealed class OptimizationProgressEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public int CurrentIteration { get; init; }

    [Key(12)]
    public double CurrentCost { get; init; }

    [Key(13)]
    public double BestCost { get; init; }

    public OptimizationProgressEvent() : base()
    {
        SourceService = "Optimization";
    }
}

[MessagePackObject]
public sealed class OptimizationCompletedEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public bool Converged { get; init; }

    [Key(13)]
    public int Iterations { get; init; }

    [Key(14)]
    public double FinalCost { get; init; }

    [Key(15)]
    public long ComputationTimeMs { get; init; }

    [Key(16)]
    public string TerminationReason { get; init; } = string.Empty;

    public OptimizationCompletedEvent() : base()
    {
        SourceService = "Optimization";
    }
}

[MessagePackObject]
public sealed class OptimizationFailedEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string ErrorMessage { get; init; } = string.Empty;

    public OptimizationFailedEvent() : base()
    {
        SourceService = "Optimization";
    }
}

[MessagePackObject]
public sealed class OptimizationCancelledEvent : EventBase
{
    [Key(10)]
    public Guid JobId { get; init; }

    public OptimizationCancelledEvent() : base()
    {
        SourceService = "Optimization";
    }
}
