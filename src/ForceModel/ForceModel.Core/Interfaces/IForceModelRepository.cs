using ForceModel.Core.Entities;

namespace ForceModel.Core.Interfaces;

public interface IForceModelConfigurationRepository
{
    Task<ForceModelConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ForceModelConfiguration?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ForceModelConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ForceModelConfiguration>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(ForceModelConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(ForceModelConfiguration configuration, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IForceModelUnitOfWork
{
    IForceModelConfigurationRepository Configurations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
