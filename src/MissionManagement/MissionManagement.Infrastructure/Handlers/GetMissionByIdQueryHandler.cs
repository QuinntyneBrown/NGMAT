using MissionManagement.Core.Entities;
using MissionManagement.Core.Interfaces;
using MissionManagement.Core.Queries;

namespace MissionManagement.Infrastructure.Handlers;

/// <summary>
/// Handles retrieving a mission by its ID.
/// </summary>
public sealed class GetMissionByIdQueryHandler
{
    private readonly IMissionRepository _repository;

    public GetMissionByIdQueryHandler(IMissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<MissionDto?> HandleAsync(
        GetMissionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var mission = await _repository.GetByIdAsync(query.MissionId, cancellationToken);
        if (mission == null)
        {
            return null;
        }

        // Check authorization
        if (!mission.IsOwnedBy(query.RequestingUserId))
        {
            throw new UnauthorizedAccessException("You do not have permission to view this mission.");
        }

        return ToDto(mission);
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
