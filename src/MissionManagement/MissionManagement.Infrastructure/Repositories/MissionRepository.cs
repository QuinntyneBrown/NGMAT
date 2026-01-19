using Microsoft.EntityFrameworkCore;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Interfaces;
using MissionManagement.Infrastructure.Persistence;

namespace MissionManagement.Infrastructure.Repositories;

public sealed class MissionRepository : IMissionRepository
{
    private readonly MissionDbContext _context;

    public MissionRepository(MissionDbContext context)
    {
        _context = context;
    }

    public async Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<Mission?> GetByIdWithSharesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .Include(m => m.Shares)
            .Include(m => m.StatusHistory)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Mission>> GetByOwnerIdAsync(
        Guid ownerId,
        int page,
        int pageSize,
        MissionStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Missions
            .Where(m => m.OwnerId == ownerId && !m.IsDeleted);

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(m => m.Name.Contains(searchTerm));

        return await query
            .OrderByDescending(m => m.UpdatedAt ?? m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Mission>> GetAccessibleByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        MissionStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Missions
            .Include(m => m.Shares)
            .Where(m => !m.IsDeleted &&
                (m.OwnerId == userId || m.Shares.Any(s => s.UserId == userId && !s.IsRevoked)));

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(m => m.Name.Contains(searchTerm));

        return await query
            .OrderByDescending(m => m.UpdatedAt ?? m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByOwnerIdAsync(
        Guid ownerId,
        MissionStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Missions
            .Where(m => m.OwnerId == ownerId && !m.IsDeleted);

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(m => m.Name.Contains(searchTerm));

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetAccessibleCountByUserIdAsync(
        Guid userId,
        MissionStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Missions
            .Include(m => m.Shares)
            .Where(m => !m.IsDeleted &&
                (m.OwnerId == userId || m.Shares.Any(s => s.UserId == userId && !s.IsRevoked)));

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(m => m.Name.Contains(searchTerm));

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAndOwnerAsync(
        string name,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .AnyAsync(m => m.Name == name && m.OwnerId == ownerId && !m.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(Mission mission, CancellationToken cancellationToken = default)
    {
        await _context.Missions.AddAsync(mission, cancellationToken);
    }

    public Task UpdateAsync(Mission mission, CancellationToken cancellationToken = default)
    {
        _context.Missions.Update(mission);
        return Task.CompletedTask;
    }
}
