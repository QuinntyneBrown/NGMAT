namespace MissionManagement.Core.Interfaces;

public interface IMissionUnitOfWork
{
    IMissionRepository Missions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
