using Maneuver.Core.Entities;

namespace Maneuver.Core.Interfaces;

public interface IManeuverPlanRepository
{
    Task<ManeuverPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManeuverPlan>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManeuverPlan>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManeuverPlan>> GetScheduledAsync(DateTime fromEpoch, DateTime toEpoch, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManeuverPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ManeuverPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(ManeuverPlan plan, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IManeuverUnitOfWork
{
    IManeuverPlanRepository ManeuverPlans { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
