using MissionManagement.Core.Enums;

namespace MissionManagement.Core.Commands;

/// <summary>
/// Command to create a new mission.
/// </summary>
public sealed record CreateMissionCommand(
    string Name,
    MissionType Type,
    DateTimeOffset StartEpoch,
    Guid OwnerId,
    string? Description = null,
    DateTimeOffset? EndEpoch = null);

/// <summary>
/// Result of creating a mission.
/// </summary>
public sealed record CreateMissionResult(Guid MissionId);
