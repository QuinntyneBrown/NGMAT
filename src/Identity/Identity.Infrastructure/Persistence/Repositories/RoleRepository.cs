using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;

    public RoleRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.Users.Any(ur => ur.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
    }

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
        if (role != null)
        {
            _context.Roles.Remove(role);
        }
    }
}
