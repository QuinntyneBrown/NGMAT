using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

internal sealed class PermissionRepository : IPermissionRepository
{
    private readonly IdentityDbContext _context;

    public PermissionRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Include(p => p.Roles)
                .ThenInclude(rp => rp.Role)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Include(p => p.Roles)
                .ThenInclude(rp => rp.Role)
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Include(p => p.Roles)
                .ThenInclude(rp => rp.Role)
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Include(p => p.Roles)
                .ThenInclude(rp => rp.Role)
            .Where(p => p.Resource == resource)
            .OrderBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Include(p => p.Roles)
                .ThenInclude(rp => rp.Role)
            .Where(p => p.Roles.Any(rp => rp.RoleId == roleId))
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }
}
