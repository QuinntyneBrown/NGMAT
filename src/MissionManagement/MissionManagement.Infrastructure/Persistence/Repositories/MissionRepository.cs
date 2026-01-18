using Microsoft.EntityFrameworkCore;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Enums;
using MissionManagement.Core.Interfaces;

namespace MissionManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Mission entity operations.
/// </summary>
internal sealed class MissionRepository : IMissionRepository
{
    private readonly MissionManagementDbContext _context;

    public MissionRepository(MissionManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Mission?> GetByIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .FirstOrDefaultAsync(m => m.MissionId == missionId, cancellationToken);
    }

    public async Task<(IEnumerable<Mission> Missions, int TotalCount)> GetByOwnerAsync(
        Guid ownerId,
        int page,
        int pageSize,
        MissionStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Missions.Where(m => m.OwnerId == ownerId);

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(term) ||
                                    (m.Description != null && m.Description.ToLower().Contains(term)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending
                ? query.OrderByDescending(m => m.Name)
                : query.OrderBy(m => m.Name),
            "createdat" => sortDescending
                ? query.OrderByDescending(m => m.CreatedAt)
                : query.OrderBy(m => m.CreatedAt),
            "updatedat" => sortDescending
                ? query.OrderByDescending(m => m.UpdatedAt)
                : query.OrderBy(m => m.UpdatedAt),
            _ => query.OrderByDescending(m => m.UpdatedAt) // Default sort
        };

        // Apply pagination
        var missions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (missions, totalCount);
    }

    public async Task<bool> ExistsAsync(
        Guid ownerId,
        string name,
        Guid? excludeMissionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Missions.Where(m => m.OwnerId == ownerId && m.Name == name);

        if (excludeMissionId.HasValue)
        {
            query = query.Where(m => m.MissionId != excludeMissionId.Value);
        }

        return await query.AnyAsync(cancellationToken);
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
