using MissionManagement.Core.Enums;

namespace MissionManagement.Core.Commands;

/// <summary>
/// Command to change a mission's status.
/// </summary>
public sealed record ChangeMissionStatusCommand(
    Guid MissionId,
    MissionStatus NewStatus,
    Guid UserId,
    string? Reason = null);
