using Microsoft.EntityFrameworkCore;
using Maneuver.Core.Entities;
using Maneuver.Core.Interfaces;
using Maneuver.Infrastructure.Persistence;

namespace Maneuver.Infrastructure.Repositories;

public sealed class ManeuverPlanRepository : IManeuverPlanRepository
{
    private readonly ManeuverDbContext _context;

    public ManeuverPlanRepository(ManeuverDbContext context)
    {
        _context = context;
    }

    public async Task<ManeuverPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ManeuverPlans
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<ManeuverPlan>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default)
    {
        return await _context.ManeuverPlans
            .Where(m => m.SpacecraftId == spacecraftId && !m.IsDeleted)
            .OrderBy(m => m.PlannedEpoch)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManeuverPlan>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.ManeuverPlans
            .Where(m => m.MissionId == missionId && !m.IsDeleted)
            .OrderBy(m => m.PlannedEpoch)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManeuverPlan>> GetScheduledAsync(DateTime fromEpoch, DateTime toEpoch, CancellationToken cancellationToken = default)
    {
        return await _context.ManeuverPlans
            .Where(m => !m.IsDeleted &&
                        m.Status == ManeuverStatus.Scheduled &&
                        m.PlannedEpoch >= fromEpoch &&
                        m.PlannedEpoch <= toEpoch)
            .OrderBy(m => m.PlannedEpoch)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManeuverPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ManeuverPlans
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.PlannedEpoch)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ManeuverPlan plan, CancellationToken cancellationToken = default)
    {
        await _context.ManeuverPlans.AddAsync(plan, cancellationToken);
    }

    public Task UpdateAsync(ManeuverPlan plan, CancellationToken cancellationToken = default)
    {
        _context.ManeuverPlans.Update(plan);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.ManeuverPlans.FindAsync(new object[] { id }, cancellationToken);
        if (plan != null)
        {
            plan.Delete();
        }
    }
}

public sealed class ManeuverUnitOfWork : IManeuverUnitOfWork
{
    private readonly ManeuverDbContext _context;

    public IManeuverPlanRepository ManeuverPlans { get; }

    public ManeuverUnitOfWork(ManeuverDbContext context)
    {
        _context = context;
        ManeuverPlans = new ManeuverPlanRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
