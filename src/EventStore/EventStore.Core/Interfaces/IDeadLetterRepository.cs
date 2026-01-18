using EventStore.Core.Entities;

namespace EventStore.Core.Interfaces;

/// <summary>
/// Repository interface for dead letter events.
/// </summary>
public interface IDeadLetterRepository
{
    /// <summary>
    /// Adds a dead letter event.
    /// </summary>
    Task<DeadLetterEvent> AddAsync(DeadLetterEvent deadLetter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unresolved dead letter events for a subscription.
    /// </summary>
    Task<IReadOnlyList<DeadLetterEvent>> GetBySubscriptionAsync(
        Guid subscriptionId,
        bool includeResolved = false,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unresolved dead letter events.
    /// </summary>
    Task<IReadOnlyList<DeadLetterEvent>> GetUnresolvedAsync(
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a dead letter event as resolved.
    /// </summary>
    Task ResolveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unresolved dead letter events.
    /// </summary>
    Task<int> GetUnresolvedCountAsync(CancellationToken cancellationToken = default);
}
