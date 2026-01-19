using Optimization.Core.Entities;

namespace Optimization.Core.Interfaces;

public interface IOptimizationJobRepository
{
    Task<OptimizationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OptimizationJob>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OptimizationJob>> GetByStatusAsync(OptimizationStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OptimizationJob>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OptimizationJob>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(OptimizationJob job, CancellationToken cancellationToken = default);
    Task UpdateAsync(OptimizationJob job, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IOptimizationUnitOfWork
{
    IOptimizationJobRepository Jobs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
