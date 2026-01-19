using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;
using Notification.Core.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

internal sealed class AlertRuleRepository : IAlertRuleRepository
{
    private readonly NotificationDbContext _context;

    public AlertRuleRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AlertRule>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AlertRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AlertRules
            .Where(a => a.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AlertRule>> GetRulesDueForEvaluationAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.AlertRules
            .Where(a => a.IsEnabled)
            .Where(a => !a.LastEvaluatedAt.HasValue ||
                        a.LastEvaluatedAt.Value.AddMinutes(a.EvaluationIntervalMinutes) <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        await _context.AlertRules.AddAsync(rule, cancellationToken);
    }

    public Task UpdateAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        _context.AlertRules.Update(rule);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await GetByIdAsync(id, cancellationToken);
        if (rule is not null)
        {
            _context.AlertRules.Remove(rule);
        }
    }
}
