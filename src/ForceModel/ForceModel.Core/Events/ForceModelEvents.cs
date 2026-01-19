using MessagePack;
using Shared.Messaging.Abstractions;

namespace ForceModel.Core.Events;

[MessagePackObject]
public sealed class ForceModelConfigurationCreatedEvent : EventBase
{
    [Key(10)]
    public Guid ConfigurationId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public Guid? MissionId { get; init; }

    public ForceModelConfigurationCreatedEvent() : base()
    {
        SourceService = "ForceModel";
    }
}

[MessagePackObject]
public sealed class ForceModelConfigurationUpdatedEvent : EventBase
{
    [Key(10)]
    public Guid ConfigurationId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string UpdatedField { get; init; } = string.Empty;

    public ForceModelConfigurationUpdatedEvent() : base()
    {
        SourceService = "ForceModel";
    }
}

[MessagePackObject]
public sealed class ForceModelConfigurationDeletedEvent : EventBase
{
    [Key(10)]
    public Guid ConfigurationId { get; init; }

    public ForceModelConfigurationDeletedEvent() : base()
    {
        SourceService = "ForceModel";
    }
}

[MessagePackObject]
public sealed class ForceCalculationPerformedEvent : EventBase
{
    [Key(10)]
    public Guid? ConfigurationId { get; init; }

    [Key(11)]
    public DateTimeOffset Epoch { get; init; }

    [Key(12)]
    public double TotalAccelerationMagnitude { get; init; }

    [Key(13)]
    public bool IncludedGravity { get; init; }

    [Key(14)]
    public bool IncludedDrag { get; init; }

    [Key(15)]
    public bool IncludedSrp { get; init; }

    [Key(16)]
    public bool IncludedThirdBody { get; init; }

    public ForceCalculationPerformedEvent() : base()
    {
        SourceService = "ForceModel";
    }
}
