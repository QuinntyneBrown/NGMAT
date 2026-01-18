namespace MissionManagement.Core.Queries;

/// <summary>
/// Query to get a mission by its ID.
/// </summary>
public sealed record GetMissionByIdQuery(
    Guid MissionId,
    Guid RequestingUserId);
