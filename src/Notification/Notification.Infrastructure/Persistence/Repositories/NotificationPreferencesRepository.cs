using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;
using Notification.Core.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

internal sealed class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly NotificationDbContext _context;

    public NotificationPreferencesRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferencesEntity?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Preferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(NotificationPreferencesEntity preferences, CancellationToken cancellationToken = default)
    {
        await _context.Preferences.AddAsync(preferences, cancellationToken);
    }

    public Task UpdateAsync(NotificationPreferencesEntity preferences, CancellationToken cancellationToken = default)
    {
        _context.Preferences.Update(preferences);
        return Task.CompletedTask;
    }
}
