using MessagePack;
using Shared.Messaging.Abstractions;
using Spacecraft.Core.Entities;

namespace Spacecraft.Core.Events;

[MessagePackObject]
public sealed class SpacecraftCreatedEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public Guid MissionId { get; init; }

    [Key(13)]
    public double DryMassKg { get; init; }

    [Key(14)]
    public double FuelMassKg { get; init; }

    [Key(15)]
    public Guid CreatedByUserId { get; init; }
}

[MessagePackObject]
public sealed class SpacecraftUpdatedEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public string? Name { get; init; }

    [Key(12)]
    public double? DryMassKg { get; init; }

    [Key(13)]
    public double? DragCoefficient { get; init; }

    [Key(14)]
    public Guid UpdatedByUserId { get; init; }
}

[MessagePackObject]
public sealed class SpacecraftDeletedEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public Guid MissionId { get; init; }

    [Key(12)]
    public Guid DeletedByUserId { get; init; }
}

[MessagePackObject]
public sealed class FuelConsumedEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public double AmountKg { get; init; }

    [Key(12)]
    public double RemainingKg { get; init; }

    [Key(13)]
    public Guid? ManeuverId { get; init; }
}

[MessagePackObject]
public sealed class AttitudeChangedEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public AttitudeMode Mode { get; init; }

    [Key(12)]
    public double Q0 { get; init; }

    [Key(13)]
    public double Q1 { get; init; }

    [Key(14)]
    public double Q2 { get; init; }

    [Key(15)]
    public double Q3 { get; init; }

    [Key(16)]
    public double SpinRateRadPerSec { get; init; }
}

[MessagePackObject]
public sealed class HardwareConfiguredEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public string HardwareType { get; init; } = string.Empty;

    [Key(12)]
    public Guid HardwareId { get; init; }

    [Key(13)]
    public string HardwareName { get; init; } = string.Empty;
}

[MessagePackObject]
public sealed class StateRecordedEvent : EventBase
{
    [Key(10)]
    public Guid SpacecraftId { get; init; }

    [Key(11)]
    public DateTime Epoch { get; init; }

    [Key(12)]
    public double X { get; init; }

    [Key(13)]
    public double Y { get; init; }

    [Key(14)]
    public double Z { get; init; }

    [Key(15)]
    public double Vx { get; init; }

    [Key(16)]
    public double Vy { get; init; }

    [Key(17)]
    public double Vz { get; init; }
}
