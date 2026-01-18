using MissionManagement.Core.Enums;

namespace MissionManagement.Core.Commands;

/// <summary>
/// Command to update an existing mission.
/// </summary>
public sealed record UpdateMissionCommand(
    Guid MissionId,
    string Name,
    MissionType Type,
    DateTimeOffset StartEpoch,
    Guid RequestingUserId,
    string? Description = null,
    DateTimeOffset? EndEpoch = null);
