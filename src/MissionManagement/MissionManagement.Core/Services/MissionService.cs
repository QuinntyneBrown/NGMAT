using MissionManagement.Core.Entities;
using MissionManagement.Core.Events;
using MissionManagement.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Core.Services;

public sealed class MissionService
{
    private readonly IMissionUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public MissionService(IMissionUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<Mission>> CreateMissionAsync(
        string name,
        MissionType type,
        DateTime startEpoch,
        Guid ownerId,
        string? description = null,
        DateTime? endEpoch = null,
        CancellationToken cancellationToken = default)
    {
        // Check name uniqueness for owner
        var exists = await _unitOfWork.Missions.ExistsByNameAndOwnerAsync(name, ownerId, cancellationToken);
        if (exists)
        {
            return Error.Conflict($"A mission with the name '{name}' already exists");
        }

        var mission = Mission.Create(name, type, startEpoch, ownerId, description, endEpoch);

        await _unitOfWork.Missions.AddAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionCreatedEvent
        {
            MissionId = mission.Id,
            Name = mission.Name,
            Type = mission.Type,
            StartEpoch = mission.StartEpoch,
            OwnerId = mission.OwnerId,
            CreatedAt = mission.CreatedAt
        }, cancellationToken);

        return mission;
    }

    public async Task<Result<Mission>> GetMissionAsync(
        Guid missionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdWithSharesAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (!mission.HasAccess(userId))
        {
            return Error.Forbidden("You do not have access to this mission");
        }

        return mission;
    }

    public async Task<Result<PagedResult<Mission>>> ListMissionsAsync(
        Guid userId,
        int page,
        int pageSize,
        MissionStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var missions = await _unitOfWork.Missions.GetAccessibleByUserIdAsync(
            userId, page, pageSize, status, searchTerm, cancellationToken);
        var totalCount = await _unitOfWork.Missions.GetAccessibleCountByUserIdAsync(
            userId, status, searchTerm, cancellationToken);

        return new PagedResult<Mission>
        {
            Items = missions,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Result<Mission>> UpdateMissionAsync(
        Guid missionId,
        Guid userId,
        string? name = null,
        string? description = null,
        DateTime? startEpoch = null,
        DateTime? endEpoch = null,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdWithSharesAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (!mission.CanEdit(userId))
        {
            return Error.Forbidden("You do not have permission to edit this mission");
        }

        // Check name uniqueness if changing name
        if (name != null && name != mission.Name)
        {
            var exists = await _unitOfWork.Missions.ExistsByNameAndOwnerAsync(name, mission.OwnerId, cancellationToken);
            if (exists)
            {
                return Error.Conflict($"A mission with the name '{name}' already exists");
            }
        }

        mission.Update(name, description, startEpoch, endEpoch);
        await _unitOfWork.Missions.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionUpdatedEvent
        {
            MissionId = mission.Id,
            Name = mission.Name,
            Description = mission.Description,
            StartEpoch = mission.StartEpoch,
            EndEpoch = mission.EndEpoch,
            UpdatedByUserId = userId,
            UpdatedAt = mission.UpdatedAt ?? DateTime.UtcNow
        }, cancellationToken);

        return mission;
    }

    public async Task<Result> DeleteMissionAsync(
        Guid missionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (mission.OwnerId != userId)
        {
            return Error.Forbidden("Only the mission owner can delete this mission");
        }

        mission.Delete();
        await _unitOfWork.Missions.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionDeletedEvent
        {
            MissionId = mission.Id,
            DeletedByUserId = userId,
            DeletedAt = mission.DeletedAt ?? DateTime.UtcNow
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Mission>> ChangeStatusAsync(
        Guid missionId,
        Guid userId,
        MissionStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (mission.OwnerId != userId)
        {
            return Error.Forbidden("Only the mission owner can change the status");
        }

        if (!mission.CanTransitionTo(newStatus))
        {
            return Error.Validation($"Cannot transition from {mission.Status} to {newStatus}");
        }

        var previousStatus = mission.Status;
        mission.TransitionTo(newStatus, userId);
        await _unitOfWork.Missions.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionStatusChangedEvent
        {
            MissionId = mission.Id,
            FromStatus = previousStatus,
            ToStatus = newStatus,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        }, cancellationToken);

        return mission;
    }

    public async Task<Result> ShareMissionAsync(
        Guid missionId,
        Guid userId,
        Guid shareWithUserId,
        MissionPermission permission,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdWithSharesAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (mission.OwnerId != userId)
        {
            return Error.Forbidden("Only the mission owner can share this mission");
        }

        if (shareWithUserId == userId)
        {
            return Error.Validation("Cannot share mission with yourself");
        }

        try
        {
            mission.AddShare(shareWithUserId, permission, userId);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict(ex.Message);
        }

        await _unitOfWork.Missions.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionSharedEvent
        {
            MissionId = mission.Id,
            SharedWithUserId = shareWithUserId,
            Permission = permission,
            SharedByUserId = userId,
            SharedAt = DateTime.UtcNow
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RevokeMissionShareAsync(
        Guid missionId,
        Guid userId,
        Guid revokeFromUserId,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdWithSharesAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (mission.OwnerId != userId)
        {
            return Error.Forbidden("Only the mission owner can revoke shares");
        }

        try
        {
            mission.RevokeShare(revokeFromUserId);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation(ex.Message);
        }

        await _unitOfWork.Missions.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionShareRevokedEvent
        {
            MissionId = mission.Id,
            RevokedFromUserId = revokeFromUserId,
            RevokedByUserId = userId,
            RevokedAt = DateTime.UtcNow
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Mission>> CloneMissionAsync(
        Guid missionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var mission = await _unitOfWork.Missions.GetByIdWithSharesAsync(missionId, cancellationToken);
        if (mission == null || mission.IsDeleted)
        {
            return Error.NotFound("Mission", missionId.ToString());
        }

        if (!mission.HasAccess(userId))
        {
            return Error.Forbidden("You do not have access to this mission");
        }

        var clonedMission = mission.Clone(userId);

        // Ensure unique name
        var baseName = clonedMission.Name;
        var counter = 1;
        while (await _unitOfWork.Missions.ExistsByNameAndOwnerAsync(clonedMission.Name, userId, cancellationToken))
        {
            counter++;
            clonedMission = Mission.Create(
                $"{baseName.Replace(" (Copy)", "")} (Copy {counter})",
                mission.Type,
                mission.StartEpoch,
                userId,
                mission.Description,
                mission.EndEpoch);
        }

        await _unitOfWork.Missions.AddAsync(clonedMission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionClonedEvent
        {
            OriginalMissionId = mission.Id,
            ClonedMissionId = clonedMission.Id,
            ClonedByUserId = userId,
            ClonedAt = DateTime.UtcNow
        }, cancellationToken);

        return clonedMission;
    }
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
