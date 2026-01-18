namespace MissionManagement.Core.Commands;

/// <summary>
/// Command to delete a mission (soft delete).
/// </summary>
public sealed record DeleteMissionCommand(
    Guid MissionId,
    Guid RequestingUserId);
