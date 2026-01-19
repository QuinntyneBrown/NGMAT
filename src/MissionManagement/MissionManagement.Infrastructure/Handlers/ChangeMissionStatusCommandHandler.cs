using MissionManagement.Core.Commands;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Interfaces;
using Shared.Contracts.Events;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Infrastructure.Handlers;

/// <summary>
/// Handles changing a mission's status.
/// </summary>
public sealed class ChangeMissionStatusCommandHandler
{
    private readonly IMissionRepository _repository;
    private readonly IEventBus _eventBus;

    public ChangeMissionStatusCommandHandler(IMissionRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(
        ChangeMissionStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        // Get the mission
        var mission = await _repository.GetByIdAsync(command.MissionId, cancellationToken);
        if (mission == null)
        {
            throw new InvalidOperationException($"Mission with ID {command.MissionId} not found.");
        }

        // Check authorization
        if (!mission.IsOwnedBy(command.UserId))
        {
            throw new UnauthorizedAccessException("Only the mission owner can change its status.");
        }

        // Store previous status for event
        var previousStatus = mission.Status;

        // Change the status
        mission.ChangeStatus(command.NewStatus);

        // Save changes
        await _repository.UpdateAsync(mission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Publish event
        var statusChangedEvent = new MissionStateChanged
        {
            MissionId = mission.MissionId,
            PreviousState = previousStatus.ToString(),
            NewState = command.NewStatus.ToString(),
            Reason = command.Reason,
            UserId = command.UserId.ToString(),
            SourceService = "MissionManagement"
        };

        await _eventBus.PublishAsync(statusChangedEvent, cancellationToken);
    }
}
