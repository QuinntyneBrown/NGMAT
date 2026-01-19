using MissionManagement.Core.Interfaces;
using MissionManagement.Infrastructure.Persistence;

namespace MissionManagement.Infrastructure.Repositories;

public sealed class MissionUnitOfWork : IMissionUnitOfWork
{
    private readonly MissionDbContext _context;
    private IMissionRepository? _missions;

    public MissionUnitOfWork(MissionDbContext context)
    {
        _context = context;
    }

    public IMissionRepository Missions => _missions ??= new MissionRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
