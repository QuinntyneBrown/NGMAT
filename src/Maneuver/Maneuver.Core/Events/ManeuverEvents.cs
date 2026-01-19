using MessagePack;
using Shared.Messaging.Abstractions;

namespace Maneuver.Core.Events;

[MessagePackObject]
public sealed class ManeuverCreatedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public Guid SpacecraftId { get; init; }

    [Key(13)]
    public string ManeuverType { get; init; } = string.Empty;

    [Key(14)]
    public DateTimeOffset PlannedEpoch { get; init; }

    [Key(15)]
    public double DeltaVMps { get; init; }

    [Key(16)]
    public double EstimatedFuelKg { get; init; }

    public ManeuverCreatedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverUpdatedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string UpdatedField { get; init; } = string.Empty;

    public ManeuverUpdatedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverScheduledEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset PlannedEpoch { get; init; }

    public ManeuverScheduledEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverStartedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset ExecutionEpoch { get; init; }

    public ManeuverStartedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverCompletedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public double ActualFuelConsumedKg { get; init; }

    [Key(13)]
    public double DeltaVAchievedMps { get; init; }

    public ManeuverCompletedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverCancelledEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    public ManeuverCancelledEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverFailedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public string FailureReason { get; init; } = string.Empty;

    public ManeuverFailedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class ManeuverDeletedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    public ManeuverDeletedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class TransferOrbitCalculatedEvent : EventBase
{
    [Key(10)]
    public Guid CalculationId { get; init; }

    [Key(11)]
    public string TransferType { get; init; } = string.Empty;

    [Key(12)]
    public double TotalDeltaVMps { get; init; }

    [Key(13)]
    public double TransferTimeSeconds { get; init; }

    [Key(14)]
    public int NumberOfBurns { get; init; }

    public TransferOrbitCalculatedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}

[MessagePackObject]
public sealed class FuelConsumedEvent : EventBase
{
    [Key(10)]
    public Guid ManeuverPlanId { get; init; }

    [Key(11)]
    public Guid SpacecraftId { get; init; }

    [Key(12)]
    public double FuelMassKg { get; init; }

    [Key(13)]
    public double RemainingFuelKg { get; init; }

    public FuelConsumedEvent() : base()
    {
        SourceService = "Maneuver";
    }
}
