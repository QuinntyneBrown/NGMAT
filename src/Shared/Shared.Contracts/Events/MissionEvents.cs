using MessagePack;
using Shared.Messaging.Abstractions;

namespace Shared.Contracts.Events;

/// <summary>
/// Event raised when a new mission is created.
/// </summary>
[MessagePackObject]
public sealed class MissionCreated : EventBase
{
    [Key(10)]
    public Guid MissionId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string? Description { get; init; }

    [Key(13)]
    public DateTimeOffset StartEpoch { get; init; }

    [Key(14)]
    public DateTimeOffset? EndEpoch { get; init; }

    [Key(15)]
    public string CreatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a mission is updated.
/// </summary>
[MessagePackObject]
public sealed class MissionUpdated : EventBase
{
    [Key(10)]
    public Guid MissionId { get; init; }

    [Key(11)]
    public string Name { get; init; } = string.Empty;

    [Key(12)]
    public string? Description { get; init; }

    [Key(13)]
    public DateTimeOffset StartEpoch { get; init; }

    [Key(14)]
    public DateTimeOffset? EndEpoch { get; init; }

    [Key(15)]
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a mission is deleted.
/// </summary>
[MessagePackObject]
public sealed class MissionDeleted : EventBase
{
    [Key(10)]
    public Guid MissionId { get; init; }

    [Key(11)]
    public string DeletedBy { get; init; } = string.Empty;
}

/// <summary>
/// Event raised when a mission's state changes.
/// </summary>
[MessagePackObject]
public sealed class MissionStateChanged : EventBase
{
    [Key(10)]
    public Guid MissionId { get; init; }

    [Key(11)]
    public string PreviousState { get; init; } = string.Empty;

    [Key(12)]
    public string NewState { get; init; } = string.Empty;

    [Key(13)]
    public string? Reason { get; init; }
}
