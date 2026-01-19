using MessagePack;
using Shared.Messaging.Abstractions;

namespace Propagation.Core.Events;

[MessagePackObject]
public sealed class PropagationConfigurationCreatedEvent : EventBase
{
    [Key(10)]
    public Guid ConfigurationId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string IntegratorType { get; init; } = string.Empty;

    public PropagationConfigurationCreatedEvent() : base()
    {
        SourceService = "Propagation";
    }
}

[MessagePackObject]
public sealed class PropagationConfigurationUpdatedEvent : EventBase
{
    [Key(10)]
    public Guid ConfigurationId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string UpdatedField { get; init; } = string.Empty;

    public PropagationConfigurationUpdatedEvent() : base()
    {
        SourceService = "Propagation";
    }
}

[MessagePackObject]
public sealed class PropagationConfigurationDeletedEvent : EventBase
{
    [Key(10)]
    public Guid ConfigurationId { get; init; }

    public PropagationConfigurationDeletedEvent() : base()
    {
        SourceService = "Propagation";
    }
}

[MessagePackObject]
public sealed class PropagationStartedEvent : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid? SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset StartEpoch { get; init; }

    [Key(13)]
    public DateTimeOffset EndEpoch { get; init; }

    public PropagationStartedEvent() : base()
    {
        SourceService = "Propagation";
    }
}

[MessagePackObject]
public sealed class PropagationCompletedEvent : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid? SpacecraftId { get; init; }

    [Key(12)]
    public DateTimeOffset StartEpoch { get; init; }

    [Key(13)]
    public DateTimeOffset EndEpoch { get; init; }

    [Key(14)]
    public int StateCount { get; init; }

    [Key(15)]
    public int StepCount { get; init; }

    [Key(16)]
    public double ComputationTimeMs { get; init; }

    [Key(17)]
    public bool WasSuccessful { get; init; }

    [Key(18)]
    public string TerminationReason { get; init; } = string.Empty;

    public PropagationCompletedEvent() : base()
    {
        SourceService = "Propagation";
    }
}

[MessagePackObject]
public sealed class PropagationFailedEvent : EventBase
{
    [Key(10)]
    public Guid PropagationId { get; init; }

    [Key(11)]
    public Guid? SpacecraftId { get; init; }

    [Key(12)]
    public string ErrorMessage { get; init; } = string.Empty;

    [Key(13)]
    public string TerminationReason { get; init; } = string.Empty;

    public PropagationFailedEvent() : base()
    {
        SourceService = "Propagation";
    }
}
