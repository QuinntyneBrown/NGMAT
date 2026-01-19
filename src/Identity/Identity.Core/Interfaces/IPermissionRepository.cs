using Identity.Core.Entities;

namespace Identity.Core.Interfaces;

/// <summary>
/// Repository interface for Permission entity.
/// </summary>
public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
