using MissionManagement.Core.Enums;

namespace MissionManagement.Api.DTOs;

/// <summary>
/// Response DTO for mission information.
/// </summary>
public sealed record MissionResponse(
    Guid MissionId,
    string Name,
    string? Description,
    MissionType Type,
    DateTimeOffset StartEpoch,
    DateTimeOffset? EndEpoch,
    MissionStatus Status,
    Guid OwnerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Response DTO for paginated mission list.
/// </summary>
public sealed record MissionListResponse(
    IEnumerable<MissionResponse> Missions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
