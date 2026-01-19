using Propagation.Core.Entities;

namespace Propagation.Core.Interfaces;

public interface IPropagationConfigurationRepository
{
    Task<PropagationConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PropagationConfiguration?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropagationConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropagationConfiguration>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(PropagationConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(PropagationConfiguration configuration, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IPropagationResultRepository
{
    Task<PropagationResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropagationResult>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropagationResult>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task AddAsync(PropagationResult result, CancellationToken cancellationToken = default);
}

public interface IPropagationUnitOfWork
{
    IPropagationConfigurationRepository Configurations { get; }
    IPropagationResultRepository Results { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
