using Microsoft.EntityFrameworkCore;
using Spacecraft.Core.Entities;
using Spacecraft.Core.Interfaces;
using Spacecraft.Infrastructure.Persistence;

namespace Spacecraft.Infrastructure.Repositories;

public sealed class SpacecraftRepository : ISpacecraftRepository
{
    private readonly SpacecraftDbContext _context;

    public SpacecraftRepository(SpacecraftDbContext context)
    {
        _context = context;
    }

    public async Task<SpacecraftEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Spacecraft
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<SpacecraftEntity?> GetByIdWithHardwareAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Spacecraft
            .Include(s => s.Thrusters)
            .Include(s => s.FuelTanks)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<SpacecraftEntity?> GetByIdWithStateHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Spacecraft
            .Include(s => s.StateHistory.OrderBy(sh => sh.Epoch))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<SpacecraftEntity>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.Spacecraft
            .Where(s => s.MissionId == missionId && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAndMissionAsync(string name, Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.Spacecraft
            .AnyAsync(s => s.Name == name && s.MissionId == missionId && !s.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(SpacecraftEntity spacecraft, CancellationToken cancellationToken = default)
    {
        await _context.Spacecraft.AddAsync(spacecraft, cancellationToken);
    }

    public Task UpdateAsync(SpacecraftEntity spacecraft, CancellationToken cancellationToken = default)
    {
        _context.Spacecraft.Update(spacecraft);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spacecraft = await _context.Spacecraft.FindAsync(new object[] { id }, cancellationToken);
        if (spacecraft != null)
        {
            spacecraft.Delete();
        }
    }
}

public sealed class SpacecraftStateRepository : ISpacecraftStateRepository
{
    private readonly SpacecraftDbContext _context;

    public SpacecraftStateRepository(SpacecraftDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SpacecraftState>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default)
    {
        return await _context.SpacecraftStates
            .Where(s => s.SpacecraftId == spacecraftId)
            .OrderBy(s => s.Epoch)
            .ToListAsync(cancellationToken);
    }

    public async Task<SpacecraftState?> GetAtEpochAsync(Guid spacecraftId, DateTime epoch, CancellationToken cancellationToken = default)
    {
        return await _context.SpacecraftStates
            .Where(s => s.SpacecraftId == spacecraftId && s.Epoch == epoch)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpacecraftState>> GetInRangeAsync(Guid spacecraftId, DateTime startEpoch, DateTime endEpoch, CancellationToken cancellationToken = default)
    {
        return await _context.SpacecraftStates
            .Where(s => s.SpacecraftId == spacecraftId && s.Epoch >= startEpoch && s.Epoch <= endEpoch)
            .OrderBy(s => s.Epoch)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SpacecraftState state, CancellationToken cancellationToken = default)
    {
        await _context.SpacecraftStates.AddAsync(state, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<SpacecraftState> states, CancellationToken cancellationToken = default)
    {
        await _context.SpacecraftStates.AddRangeAsync(states, cancellationToken);
    }
}

public sealed class SpacecraftUnitOfWork : ISpacecraftUnitOfWork
{
    private readonly SpacecraftDbContext _context;

    public ISpacecraftRepository Spacecraft { get; }
    public ISpacecraftStateRepository SpacecraftStates { get; }

    public SpacecraftUnitOfWork(SpacecraftDbContext context)
    {
        _context = context;
        Spacecraft = new SpacecraftRepository(context);
        SpacecraftStates = new SpacecraftStateRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
