using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a spacecraft is created.
/// </summary>
[MessagePackObject]
public sealed class SpacecraftCreated : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public Guid MissionId { get; init; }

    [Key(12)]
    public string Name { get; init; } = string.Empty;

    [Key(13)]
    public double DryMass { get; init; }

    [Key(14)]
    public double FuelMass { get; init; }

    [Key(15)]
    public string CreatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a spacecraft is updated.
/// </summary>
[MessagePackObject]
public sealed class SpacecraftUpdated : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public double DryMass { get; init; }

    [Key(13)]
    public double FuelMass { get; init; }

    [Key(14)]
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a spacecraft is deleted.
/// </summary>
[MessagePackObject]
public sealed class SpacecraftDeleted : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public Guid MissionId { get; init; }

    [Key(12)]
    public string DeletedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a spacecraft's state vector is updated.
/// </summary>
[MessagePackObject]
public sealed class StateVectorUpdated : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public DateTimeOffset Epoch { get; init; }

    [Key(12)]
    public double PositionX { get; init; }

    [Key(13)]
    public double PositionY { get; init; }

    [Key(14)]
    public double PositionZ { get; init; }

    [Key(15)]
    public double VelocityX { get; init; }

    [Key(16)]
    public double VelocityY { get; init; }

    [Key(17)]
    public double VelocityZ { get; init; }

    [Key(18)]
    public string CoordinateSystem { get; init; } = string.Empty;
}
