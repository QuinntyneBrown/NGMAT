using Notification.Core.Entities;

namespace Notification.Core.Interfaces;

/// <summary>
/// Repository for alert rules
/// </summary>
public interface IAlertRuleRepository
{
    Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertRule>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertRule>> GetRulesDueForEvaluationAsync(CancellationToken cancellationToken = default);
    Task AddAsync(AlertRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(AlertRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
