using Microsoft.EntityFrameworkCore;
using Propagation.Core.Entities;
using Propagation.Core.Interfaces;
using Propagation.Infrastructure.Persistence;

namespace Propagation.Infrastructure.Repositories;

public sealed class PropagationConfigurationRepository : IPropagationConfigurationRepository
{
    private readonly PropagationDbContext _context;

    public PropagationConfigurationRepository(PropagationDbContext context)
    {
        _context = context;
    }

    public async Task<PropagationConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PropagationConfigurations
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<PropagationConfiguration?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.PropagationConfigurations
            .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<PropagationConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PropagationConfigurations
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PropagationConfiguration>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.PropagationConfigurations
            .Where(c => c.MissionId == missionId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.PropagationConfigurations
            .AnyAsync(c => c.Name == name && !c.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(PropagationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _context.PropagationConfigurations.AddAsync(configuration, cancellationToken);
    }

    public Task UpdateAsync(PropagationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _context.PropagationConfigurations.Update(configuration);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await _context.PropagationConfigurations.FindAsync(new object[] { id }, cancellationToken);
        if (config != null)
        {
            config.Delete();
        }
    }
}

// In-memory implementation for PropagationResult (not persisted to DB in this version)
public sealed class InMemoryPropagationResultRepository : IPropagationResultRepository
{
    private readonly List<PropagationResult> _results = new();
    private readonly object _lock = new();

    public Task<PropagationResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var result = _results.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<PropagationResult>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var results = _results
                .Where(r => r.SpacecraftId == spacecraftId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            return Task.FromResult<IReadOnlyList<PropagationResult>>(results);
        }
    }

    public Task<IReadOnlyList<PropagationResult>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var results = _results
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToList();
            return Task.FromResult<IReadOnlyList<PropagationResult>>(results);
        }
    }

    public Task AddAsync(PropagationResult result, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Keep only last 100 results to prevent memory issues
            while (_results.Count >= 100)
            {
                _results.RemoveAt(0);
            }
            _results.Add(result);
        }
        return Task.CompletedTask;
    }
}

public sealed class PropagationUnitOfWork : IPropagationUnitOfWork
{
    private readonly PropagationDbContext _context;

    public IPropagationConfigurationRepository Configurations { get; }
    public IPropagationResultRepository Results { get; }

    public PropagationUnitOfWork(PropagationDbContext context, IPropagationResultRepository resultRepository)
    {
        _context = context;
        Configurations = new PropagationConfigurationRepository(context);
        Results = resultRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
