using MissionManagement.Core.Entities;
using MissionManagement.Core.Interfaces;
using MissionManagement.Core.Queries;

namespace MissionManagement.Infrastructure.Handlers;

/// <summary>
/// Handles listing missions with filtering and pagination.
/// </summary>
public sealed class ListMissionsQueryHandler
{
    private readonly IMissionRepository _repository;

    public ListMissionsQueryHandler(IMissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListMissionsResult> HandleAsync(
        ListMissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var (missions, totalCount) = await _repository.GetByOwnerAsync(
            query.OwnerId,
            query.Page,
            query.PageSize,
            query.Status,
            query.SearchTerm,
            query.SortBy,
            query.SortDescending,
            cancellationToken);

        var missionDtos = missions.Select(ToDto).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new ListMissionsResult(
            missionDtos,
            totalCount,
            query.Page,
            query.PageSize,
            totalPages);
    }

    private static MissionDto ToDto(Mission mission)
    {
        return new MissionDto(
            mission.MissionId,
            mission.Name,
            mission.Description,
            mission.Type,
            mission.StartEpoch,
            mission.EndEpoch,
            mission.Status,
            mission.OwnerId,
            mission.CreatedAt,
            mission.UpdatedAt);
    }
}
