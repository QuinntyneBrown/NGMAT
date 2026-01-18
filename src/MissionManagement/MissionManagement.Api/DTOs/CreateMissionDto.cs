using MissionManagement.Core.Enums;

namespace MissionManagement.Api.DTOs;

/// <summary>
/// Request DTO for creating a new mission.
/// </summary>
public sealed record CreateMissionRequest(
    string Name,
    MissionType Type,
    DateTimeOffset StartEpoch,
    string? Description = null,
    DateTimeOffset? EndEpoch = null);

/// <summary>
/// Response DTO for mission creation.
/// </summary>
public sealed record CreateMissionResponse(Guid MissionId);
