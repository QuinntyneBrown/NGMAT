using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a maneuver is planned.
/// </summary>
[MessagePackObject]
public sealed class ManeuverPlanned : EventBase
{
    [Key(10)]
    public Guid ManeuverId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string ManeuverType { get; init; } = string.Empty;

    [Key(13)]
    public DateTimeOffset PlannedEpoch { get; init; }

    [Key(14)]
    public double DeltaVX { get; init; }

    [Key(15)]
    public double DeltaVY { get; init; }

    [Key(16)]
    public double DeltaVZ { get; init; }

    [Key(17)]
    public double Duration { get; init; }

    [Key(18)]
    public string PlannedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a maneuver is updated.
/// </summary>
[MessagePackObject]
public sealed class ManeuverUpdated : EventBase
{
    [Key(10)]
    public Guid ManeuverId { get; init; }

    [Key(11)]
    public DateTimeOffset PlannedEpoch { get; init; }

    [Key(12)]
    public double DeltaVX { get; init; }

    [Key(13)]
    public double DeltaVY { get; init; }

    [Key(14)]
    public double DeltaVZ { get; init; }

    [Key(15)]
    public double Duration { get; init; }

    [Key(16)]
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a maneuver is executed (applied to propagation).
/// </summary>
[MessagePackObject]
public sealed class ManeuverExecuted : EventBase
{
    [Key(10)]
    public Guid ManeuverId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset ExecutedAt { get; init; }

    [Key(13)]
    public double ActualDeltaVX { get; init; }

    [Key(14)]
    public double ActualDeltaVY { get; init; }

    [Key(15)]
    public double ActualDeltaVZ { get; init; }

    [Key(16)]
    public double FuelConsumed { get; init; }
}

/// <summary>
/// Event raised when a maneuver is deleted.
/// </summary>
[MessagePackObject]
public sealed class ManeuverDeleted : EventBase
{
    [Key(10)]
    public Guid ManeuverId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string DeletedBy { get; init; } = string.Empty;
}
