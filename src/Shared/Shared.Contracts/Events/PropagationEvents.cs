using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a propagation is requested.
/// </summary>
[MessagePackObject]
public sealed class PropagationRequested : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset StartEpoch { get; init; }

    [Key(13)]
    public DateTimeOffset EndEpoch { get; init; }

    [Key(14)]
    public double StepSize { get; init; }

    [Key(15)]
    public string PropagatorType { get; init; } = string.Empty;

    [Key(16)]
    public string RequestedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a propagation starts processing.
/// </summary>
[MessagePackObject]
public sealed class PropagationStarted : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset StartedAt { get; init; }
}

/// <summary>
/// Event raised to report propagation progress.
/// </summary>
[MessagePackObject]
public sealed class PropagationProgress : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public double PercentComplete { get; init; }

    [Key(12)]
    public DateTimeOffset CurrentEpoch { get; init; }

    [Key(13)]
    public int StepsCompleted { get; init; }

    [Key(14)]
    public int TotalSteps { get; init; }
}

/// <summary>
/// Event raised when a propagation completes successfully.
/// </summary>
[MessagePackObject]
public sealed class PropagationCompleted : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset CompletedAt { get; init; }

    [Key(13)]
    public int TotalSteps { get; init; }

    [Key(14)]
    public double ElapsedMilliseconds { get; init; }

    [Key(15)]
    public string? ResultLocation { get; init; }
}

/// <summary>
/// Event raised when a propagation fails.
/// </summary>
[MessagePackObject]
public sealed class PropagationFailed : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string ErrorCode { get; init; } = string.Empty;

    [Key(13)]
    public string ErrorMessage { get; init; } = string.Empty;

    [Key(14)]
    public DateTimeOffset? FailedAtEpoch { get; init; }
}

/// <summary>
/// Event raised when a propagation is cancelled.
/// </summary>
[MessagePackObject]
public sealed class PropagationCancelled : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string CancelledBy { get; init; } = string.Empty;

    [Key(13)]
    public string? Reason { get; init; }
}
