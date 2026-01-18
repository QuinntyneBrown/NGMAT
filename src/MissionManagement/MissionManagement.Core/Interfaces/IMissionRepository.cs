using MissionManagement.Core.Entities;
using MissionManagement.Core.Enums;

namespace MissionManagement.Core.Interfaces;

/// <summary>
/// Repository interface for Mission entity operations.
/// </summary>
public interface IMissionRepository
{
    /// <summary>
    /// Gets a mission by its ID.
    /// </summary>
    Task<Mission?> GetByIdAsync(Guid missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets missions for a specific owner with optional filtering.
    /// </summary>
    Task<(IEnumerable<Mission> Missions, int TotalCount)> GetByOwnerAsync(
        Guid ownerId,
        int page,
        int pageSize,
        MissionStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a mission name already exists for a specific owner.
    /// </summary>
    Task<bool> ExistsAsync(Guid ownerId, string name, Guid? excludeMissionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new mission.
    /// </summary>
    Task AddAsync(Mission mission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mission.
    /// </summary>
    Task UpdateAsync(Mission mission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
