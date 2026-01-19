using MissionManagement.Core.Enums;

namespace MissionManagement.Api.DTOs;

/// <summary>
/// Request DTO for changing a mission's status.
/// </summary>
public sealed record ChangeMissionStatusRequest(
    MissionStatus Status,
    string? Reason = null);
