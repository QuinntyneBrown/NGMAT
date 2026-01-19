using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;
using Notification.Core.Interfaces;
using Notification.Core.Models;

namespace Notification.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(
        string userId,
        NotificationFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (filter is not null)
        {
            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            if (filter.Channel.HasValue)
                query = query.Where(n => n.Channel == filter.Channel.Value);

            if (filter.UnreadOnly == true)
                query = query.Where(n => n.Status != NotificationStatus.Read);

            if (filter.FromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);

            query = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(filter.Skip)
                .Take(filter.Take);
        }
        else
        {
            query = query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationEntity>> GetPendingAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationEntity>> GetFailedForRetryAsync(
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < maxRetries)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.Status != NotificationStatus.Read, cancellationToken);
    }

    public async Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public Task UpdateAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(id, cancellationToken);
        if (notification is not null)
        {
            _context.Notifications.Remove(notification);
        }
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(id, cancellationToken);
        if (notification is not null)
        {
            notification.MarkAsRead();
        }
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.Status != NotificationStatus.Read)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }
    }
}
