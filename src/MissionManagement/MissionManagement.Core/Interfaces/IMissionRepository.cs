using MissionManagement.Core.Entities;

namespace MissionManagement.Core.Interfaces;

public interface IMissionRepository
{
    Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Mission?> GetByIdWithSharesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mission>> GetByOwnerIdAsync(Guid ownerId, int page, int pageSize, MissionStatus? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mission>> GetAccessibleByUserIdAsync(Guid userId, int page, int pageSize, MissionStatus? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByOwnerIdAsync(Guid ownerId, MissionStatus? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<int> GetAccessibleCountByUserIdAsync(Guid userId, MissionStatus? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Mission mission, CancellationToken cancellationToken = default);
    Task UpdateAsync(Mission mission, CancellationToken cancellationToken = default);
}
