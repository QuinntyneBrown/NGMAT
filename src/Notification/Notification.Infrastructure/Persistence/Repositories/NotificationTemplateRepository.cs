using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Infrastructure.Persistence.Repositories;

internal sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _context;

    public NotificationTemplateRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<NotificationTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationTemplate>> GetByChannelAsync(
        NotificationChannel channel,
        CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Where(t => t.Channel == channel && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Templates.AsQueryable();

        if (activeOnly)
            query = query.Where(t => t.IsActive);

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.Templates.AddAsync(template, cancellationToken);
    }

    public Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        _context.Templates.Update(template);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(id, cancellationToken);
        if (template is not null)
        {
            _context.Templates.Remove(template);
        }
    }
}
