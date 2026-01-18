using MissionManagement.Core.Commands;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Interfaces;
using Shared.Contracts.Events;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Infrastructure.Handlers;

/// <summary>
/// Handles the creation of a new mission.
/// </summary>
public sealed class CreateMissionCommandHandler
{
    private readonly IMissionRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateMissionCommandHandler(IMissionRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<CreateMissionResult> HandleAsync(
        CreateMissionCommand command,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate mission name
        var exists = await _repository.ExistsAsync(command.OwnerId, command.Name, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"A mission with name '{command.Name}' already exists for this user.");
        }

        // Create the mission
        var mission = Mission.Create(
            command.Name,
            command.Type,
            command.StartEpoch,
            command.OwnerId,
            command.Description,
            command.EndEpoch);

        await _repository.AddAsync(mission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Publish event
        var missionCreatedEvent = new MissionCreated
        {
            MissionId = mission.MissionId,
            Name = mission.Name,
            Description = mission.Description,
            StartEpoch = mission.StartEpoch,
            EndEpoch = mission.EndEpoch,
            CreatedBy = command.OwnerId.ToString(),
            UserId = command.OwnerId.ToString(),
            SourceService = "MissionManagement"
        };

        await _eventBus.PublishAsync(missionCreatedEvent, cancellationToken);

        return new CreateMissionResult(mission.MissionId);
    }
}
