using MissionManagement.Core.Entities;
using MissionManagement.Core.Events;
using MissionManagement.Core.Interfaces;
using MissionManagement.Core.Models;
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

    public async Task<Result<MissionExportData>> ExportMissionAsync(
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

        var exportData = new MissionExportData
        {
            Version = 1,
            ExportedAt = DateTime.UtcNow.ToString("O"),
            ExportedBy = userId.ToString(),
            Mission = mission.ToExportData()
        };

        await _eventPublisher.PublishAsync(new MissionExportedEvent
        {
            MissionId = mission.Id,
            MissionName = mission.Name,
            ExportedByUserId = userId,
            ExportedAt = DateTime.UtcNow,
            ExportFormat = "JSON"
        }, cancellationToken);

        return exportData;
    }

    public async Task<Result<MissionBatchExportData>> ExportMissionsAsync(
        Guid userId,
        IEnumerable<Guid>? missionIds = null,
        MissionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        List<Mission> missions;

        if (missionIds != null && missionIds.Any())
        {
            // Export specific missions
            missions = new List<Mission>();
            foreach (var id in missionIds)
            {
                var mission = await _unitOfWork.Missions.GetByIdWithSharesAsync(id, cancellationToken);
                if (mission != null && !mission.IsDeleted && mission.HasAccess(userId))
                {
                    missions.Add(mission);
                }
            }
        }
        else
        {
            // Export all accessible missions with optional status filter
            missions = (await _unitOfWork.Missions.GetAccessibleByUserIdAsync(
                userId, 1, int.MaxValue, status, null, cancellationToken)).ToList();
        }

        var exportData = new MissionBatchExportData
        {
            Version = 1,
            ExportedAt = DateTime.UtcNow.ToString("O"),
            ExportedBy = userId.ToString(),
            MissionCount = missions.Count,
            Missions = missions.Select(m => m.ToExportData()).ToList()
        };

        await _eventPublisher.PublishAsync(new MissionBatchExportedEvent
        {
            MissionCount = missions.Count,
            ExportedByUserId = userId,
            ExportedAt = DateTime.UtcNow,
            ExportFormat = "JSON"
        }, cancellationToken);

        return exportData;
    }

    public async Task<Result<MissionImportResult>> ImportMissionAsync(
        Guid userId,
        MissionData missionData,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        // Validate mission type
        var (missionType, typeValid) = MissionExportExtensions.ParseMissionType(missionData.Type);
        if (!typeValid)
        {
            return new MissionImportResult
            {
                MissionId = Guid.Empty,
                MissionName = missionData.Name,
                Success = false,
                ErrorMessage = $"Invalid mission type: {missionData.Type}"
            };
        }

        // Check if mission with same name exists
        var exists = await _unitOfWork.Missions.ExistsByNameAndOwnerAsync(missionData.Name, userId, cancellationToken);
        bool wasOverwritten = false;

        if (exists)
        {
            if (!overwriteExisting)
            {
                return new MissionImportResult
                {
                    MissionId = Guid.Empty,
                    MissionName = missionData.Name,
                    Success = false,
                    ErrorMessage = $"A mission with the name '{missionData.Name}' already exists. Set overwriteExisting to true to replace it."
                };
            }

            // Find and delete existing mission
            var existingMissions = await _unitOfWork.Missions.GetByOwnerIdAsync(userId, 1, 1, null, missionData.Name, cancellationToken);
            var existingMission = existingMissions.FirstOrDefault(m => m.Name == missionData.Name);
            if (existingMission != null)
            {
                existingMission.Delete();
                await _unitOfWork.Missions.UpdateAsync(existingMission, cancellationToken);
                wasOverwritten = true;
            }
        }

        // Create new mission from import data
        var mission = Mission.Create(
            missionData.Name,
            missionType,
            missionData.StartEpoch,
            userId,
            missionData.Description,
            missionData.EndEpoch);

        await _unitOfWork.Missions.AddAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MissionImportedEvent
        {
            MissionId = mission.Id,
            MissionName = mission.Name,
            ImportedByUserId = userId,
            ImportedAt = DateTime.UtcNow,
            WasOverwritten = wasOverwritten
        }, cancellationToken);

        return new MissionImportResult
        {
            MissionId = mission.Id,
            MissionName = mission.Name,
            Success = true,
            WasOverwritten = wasOverwritten
        };
    }

    public async Task<Result<MissionBatchImportResult>> ImportMissionsAsync(
        Guid userId,
        List<MissionData> missions,
        bool overwriteExisting = false,
        bool stopOnError = true,
        CancellationToken cancellationToken = default)
    {
        var results = new List<MissionImportResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var missionData in missions)
        {
            var importResult = await ImportMissionAsync(userId, missionData, overwriteExisting, cancellationToken);

            if (importResult.IsSuccess)
            {
                results.Add(importResult.Value);
                if (importResult.Value.Success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                    if (stopOnError)
                    {
                        break;
                    }
                }
            }
            else
            {
                results.Add(new MissionImportResult
                {
                    MissionId = Guid.Empty,
                    MissionName = missionData.Name,
                    Success = false,
                    ErrorMessage = importResult.Error.Message
                });
                failureCount++;
                if (stopOnError)
                {
                    break;
                }
            }
        }

        var batchResult = new MissionBatchImportResult
        {
            TotalCount = missions.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };

        await _eventPublisher.PublishAsync(new MissionBatchImportedEvent
        {
            TotalCount = missions.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            ImportedByUserId = userId,
            ImportedAt = DateTime.UtcNow
        }, cancellationToken);

        return batchResult;
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
