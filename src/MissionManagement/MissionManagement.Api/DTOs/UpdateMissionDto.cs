using MissionManagement.Core.Enums;

namespace MissionManagement.Api.DTOs;

/// <summary>
/// Request DTO for updating a mission.
/// </summary>
public sealed record UpdateMissionRequest(
    string Name,
    MissionType Type,
    DateTimeOffset StartEpoch,
    string? Description = null,
    DateTimeOffset? EndEpoch = null);
