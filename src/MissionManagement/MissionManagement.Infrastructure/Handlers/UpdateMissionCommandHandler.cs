using MissionManagement.Core.Commands;
using MissionManagement.Core.Interfaces;
using Shared.Contracts.Events;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Infrastructure.Handlers;

/// <summary>
/// Handles updating an existing mission.
/// </summary>
public sealed class UpdateMissionCommandHandler
{
    private readonly IMissionRepository _repository;
    private readonly IEventBus _eventBus;

    public UpdateMissionCommandHandler(IMissionRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(
        UpdateMissionCommand command,
        CancellationToken cancellationToken = default)
    {
        // Get the mission
        var mission = await _repository.GetByIdAsync(command.MissionId, cancellationToken);
        if (mission == null)
        {
            throw new InvalidOperationException($"Mission with ID '{command.MissionId}' not found.");
        }

        // Check authorization
        if (!mission.IsOwnedBy(command.RequestingUserId))
        {
            throw new UnauthorizedAccessException("Only the mission owner can update the mission.");
        }

        // Check for duplicate name if name is changing
        if (mission.Name != command.Name)
        {
            var exists = await _repository.ExistsAsync(
                command.RequestingUserId,
                command.Name,
                command.MissionId,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"A mission with name '{command.Name}' already exists for this user.");
            }
        }

        // Update the mission
        mission.Update(
            command.Name,
            command.Type,
            command.StartEpoch,
            command.Description,
            command.EndEpoch);

        await _repository.UpdateAsync(mission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Publish event
        var missionUpdatedEvent = new MissionUpdated
        {
            MissionId = mission.MissionId,
            Name = mission.Name,
            Description = mission.Description,
            StartEpoch = mission.StartEpoch,
            EndEpoch = mission.EndEpoch,
            UpdatedBy = command.RequestingUserId.ToString(),
            UserId = command.RequestingUserId.ToString(),
            SourceService = "MissionManagement"
        };

        await _eventBus.PublishAsync(missionUpdatedEvent, cancellationToken);
    }
}
