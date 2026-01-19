using Optimization.Core.Entities;
using Optimization.Core.Interfaces;

namespace Optimization.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of optimization job repository
/// </summary>
public sealed class InMemoryOptimizationJobRepository : IOptimizationJobRepository
{
    private readonly List<OptimizationJob> _jobs = new();
    private readonly object _lock = new();

    public Task<OptimizationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id && !j.IsDeleted);
            return Task.FromResult(job);
        }
    }

    public Task<IReadOnlyList<OptimizationJob>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var jobs = _jobs
                .Where(j => j.MissionId == missionId && !j.IsDeleted)
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
            return Task.FromResult<IReadOnlyList<OptimizationJob>>(jobs);
        }
    }

    public Task<IReadOnlyList<OptimizationJob>> GetByStatusAsync(OptimizationStatus status, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var jobs = _jobs
                .Where(j => j.Status == status && !j.IsDeleted)
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
            return Task.FromResult<IReadOnlyList<OptimizationJob>>(jobs);
        }
    }

    public Task<IReadOnlyList<OptimizationJob>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var jobs = _jobs
                .Where(j => !j.IsDeleted)
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .ToList();
            return Task.FromResult<IReadOnlyList<OptimizationJob>>(jobs);
        }
    }

    public Task<IReadOnlyList<OptimizationJob>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var jobs = _jobs
                .Where(j => !j.IsDeleted)
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
            return Task.FromResult<IReadOnlyList<OptimizationJob>>(jobs);
        }
    }

    public Task AddAsync(OptimizationJob job, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Keep only last 100 jobs
            while (_jobs.Count >= 100)
            {
                var oldest = _jobs.Where(j => j.IsDeleted || j.Status == OptimizationStatus.Converged || j.Status == OptimizationStatus.Failed)
                    .OrderBy(j => j.CreatedAt)
                    .FirstOrDefault();
                if (oldest != null)
                {
                    _jobs.Remove(oldest);
                }
                else
                {
                    break;
                }
            }
            _jobs.Add(job);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(OptimizationJob job, CancellationToken cancellationToken = default)
    {
        // In-memory - no action needed as object is already updated
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            job?.Delete();
        }
        return Task.CompletedTask;
    }
}

public sealed class InMemoryOptimizationUnitOfWork : IOptimizationUnitOfWork
{
    public IOptimizationJobRepository Jobs { get; }

    public InMemoryOptimizationUnitOfWork()
    {
        Jobs = new InMemoryOptimizationJobRepository();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In-memory - changes are saved immediately
        return Task.FromResult(1);
    }
}
