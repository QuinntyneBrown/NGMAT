using EventStore.Core.Entities;

namespace EventStore.Core.Interfaces;

/// <summary>
/// Repository interface for subscriptions.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Adds a subscription.
    /// </summary>
    Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription by ID.
    /// </summary>
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription by name.
    /// </summary>
    Task<Subscription?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active subscriptions.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscriptions interested in a specific event type.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscriptions due for retry.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetDueForRetryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a subscription.
    /// </summary>
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a subscription.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
