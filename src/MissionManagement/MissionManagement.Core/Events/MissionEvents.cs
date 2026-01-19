using MessagePack;
using MissionManagement.Core.Entities;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Core.Events;

[MessagePackObject]
public sealed class MissionCreatedEvent : EventBase
{
    [Key(10)]
    public required Guid MissionId { get; init; }

    [Key(11)]
    public required string Name { get; init; }

    [Key(12)]
    public required MissionType Type { get; init; }

    [Key(13)]
    public required DateTime StartEpoch { get; init; }

    [Key(14)]
    public required Guid OwnerId { get; init; }

    [Key(15)]
    public required DateTime CreatedAt { get; init; }
}

[MessagePackObject]
public sealed class MissionUpdatedEvent : EventBase
{
    [Key(10)]
    public required Guid MissionId { get; init; }

    [Key(11)]
    public required string Name { get; init; }

    [Key(12)]
    public string? Description { get; init; }

    [Key(13)]
    public required DateTime StartEpoch { get; init; }

    [Key(14)]
    public DateTime? EndEpoch { get; init; }

    [Key(15)]
    public required Guid UpdatedByUserId { get; init; }

    [Key(16)]
    public required DateTime UpdatedAt { get; init; }
}

[MessagePackObject]
public sealed class MissionDeletedEvent : EventBase
{
    [Key(10)]
    public required Guid MissionId { get; init; }

    [Key(11)]
    public required Guid DeletedByUserId { get; init; }

    [Key(12)]
    public required DateTime DeletedAt { get; init; }
}

[MessagePackObject]
public sealed class MissionStatusChangedEvent : EventBase
{
    [Key(10)]
    public required Guid MissionId { get; init; }

    [Key(11)]
    public required MissionStatus FromStatus { get; init; }

    [Key(12)]
    public required MissionStatus ToStatus { get; init; }

    [Key(13)]
    public required Guid ChangedByUserId { get; init; }

    [Key(14)]
    public required DateTime ChangedAt { get; init; }
}

[MessagePackObject]
public sealed class MissionSharedEvent : EventBase
{
    [Key(10)]
    public required Guid MissionId { get; init; }

    [Key(11)]
    public required Guid SharedWithUserId { get; init; }

    [Key(12)]
    public required MissionPermission Permission { get; init; }

    [Key(13)]
    public required Guid SharedByUserId { get; init; }

    [Key(14)]
    public required DateTime SharedAt { get; init; }
}

[MessagePackObject]
public sealed class MissionShareRevokedEvent : EventBase
{
    [Key(10)]
    public required Guid MissionId { get; init; }

    [Key(11)]
    public required Guid RevokedFromUserId { get; init; }

    [Key(12)]
    public required Guid RevokedByUserId { get; init; }

    [Key(13)]
    public required DateTime RevokedAt { get; init; }
}

[MessagePackObject]
public sealed class MissionClonedEvent : EventBase
{
    [Key(10)]
    public required Guid OriginalMissionId { get; init; }

    [Key(11)]
    public required Guid ClonedMissionId { get; init; }

    [Key(12)]
    public required Guid ClonedByUserId { get; init; }

    [Key(13)]
    public required DateTime ClonedAt { get; init; }
}
