using Spacecraft.Core.Entities;

namespace Spacecraft.Core.Interfaces;

public interface ISpacecraftRepository
{
    Task<SpacecraftEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SpacecraftEntity?> GetByIdWithHardwareAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SpacecraftEntity?> GetByIdWithStateHistoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpacecraftEntity>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndMissionAsync(string name, Guid missionId, CancellationToken cancellationToken = default);
    Task AddAsync(SpacecraftEntity spacecraft, CancellationToken cancellationToken = default);
    Task UpdateAsync(SpacecraftEntity spacecraft, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ISpacecraftStateRepository
{
    Task<IReadOnlyList<SpacecraftState>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default);
    Task<SpacecraftState?> GetAtEpochAsync(Guid spacecraftId, DateTime epoch, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpacecraftState>> GetInRangeAsync(Guid spacecraftId, DateTime startEpoch, DateTime endEpoch, CancellationToken cancellationToken = default);
    Task AddAsync(SpacecraftState state, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<SpacecraftState> states, CancellationToken cancellationToken = default);
}

public interface ISpacecraftUnitOfWork
{
    ISpacecraftRepository Spacecraft { get; }
    ISpacecraftStateRepository SpacecraftStates { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
