using MissionManagement.Core.Commands;
using MissionManagement.Core.Interfaces;
using Shared.Contracts.Events;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Infrastructure.Handlers;

/// <summary>
/// Handles soft deletion of a mission.
/// </summary>
public sealed class DeleteMissionCommandHandler
{
    private readonly IMissionRepository _repository;
    private readonly IEventBus _eventBus;

    public DeleteMissionCommandHandler(IMissionRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(
        DeleteMissionCommand command,
        CancellationToken cancellationToken = default)
    {
        // Get the mission (including soft-deleted ones for this operation)
        var mission = await _repository.GetByIdAsync(command.MissionId, cancellationToken);
        if (mission == null)
        {
            throw new InvalidOperationException($"Mission with ID '{command.MissionId}' not found.");
        }

        // Check authorization
        if (!mission.IsOwnedBy(command.RequestingUserId))
        {
            throw new UnauthorizedAccessException("Only the mission owner can delete the mission.");
        }

        // Soft delete the mission
        mission.Delete();

        await _repository.UpdateAsync(mission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Publish event
        var missionDeletedEvent = new MissionDeleted
        {
            MissionId = mission.MissionId,
            DeletedBy = command.RequestingUserId.ToString(),
            UserId = command.RequestingUserId.ToString(),
            SourceService = "MissionManagement"
        };

        await _eventBus.PublishAsync(missionDeletedEvent, cancellationToken);
    }
}
