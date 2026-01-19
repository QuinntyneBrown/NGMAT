using Microsoft.EntityFrameworkCore;
using ForceModel.Core.Entities;
using ForceModel.Core.Interfaces;
using ForceModel.Infrastructure.Persistence;

namespace ForceModel.Infrastructure.Repositories;

public sealed class ForceModelConfigurationRepository : IForceModelConfigurationRepository
{
    private readonly ForceModelDbContext _context;

    public ForceModelConfigurationRepository(ForceModelDbContext context)
    {
        _context = context;
    }

    public async Task<ForceModelConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ForceModelConfigurations
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<ForceModelConfiguration?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.ForceModelConfigurations
            .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<ForceModelConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ForceModelConfigurations
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ForceModelConfiguration>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.ForceModelConfigurations
            .Where(c => c.MissionId == missionId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.ForceModelConfigurations
            .AnyAsync(c => c.Name == name && !c.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(ForceModelConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _context.ForceModelConfigurations.AddAsync(configuration, cancellationToken);
    }

    public Task UpdateAsync(ForceModelConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _context.ForceModelConfigurations.Update(configuration);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await _context.ForceModelConfigurations.FindAsync(new object[] { id }, cancellationToken);
        if (config != null)
        {
            config.Delete();
        }
    }
}

public sealed class ForceModelUnitOfWork : IForceModelUnitOfWork
{
    private readonly ForceModelDbContext _context;

    public IForceModelConfigurationRepository Configurations { get; }

    public ForceModelUnitOfWork(ForceModelDbContext context)
    {
        _context = context;
        Configurations = new ForceModelConfigurationRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
