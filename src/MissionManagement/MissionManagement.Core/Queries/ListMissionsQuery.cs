using MissionManagement.Core.Enums;

namespace MissionManagement.Core.Queries;

/// <summary>
/// Query to list missions for a user with optional filtering and pagination.
/// </summary>
public sealed record ListMissionsQuery(
    Guid OwnerId,
    int Page = 1,
    int PageSize = 20,
    MissionStatus? Status = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false);

/// <summary>
/// Result of listing missions with pagination information.
/// </summary>
public sealed record ListMissionsResult(
    IEnumerable<MissionDto> Missions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// DTO for mission information.
/// </summary>
public sealed record MissionDto(
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
