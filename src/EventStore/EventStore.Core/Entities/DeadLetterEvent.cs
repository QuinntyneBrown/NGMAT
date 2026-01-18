namespace EventStore.Core.Entities;

/// <summary>
/// Represents an event that failed delivery and is in the dead letter queue.
/// </summary>
public sealed class DeadLetterEvent
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public int AttemptCount { get; private set; }
    public DateTimeOffset FirstAttemptAt { get; private set; }
    public DateTimeOffset LastAttemptAt { get; private set; }
    public bool IsResolved { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    private DeadLetterEvent() { } // For EF Core

    public static DeadLetterEvent Create(
        Guid eventId,
        Guid subscriptionId,
        string errorMessage,
        int attemptCount)
    {
        var now = DateTimeOffset.UtcNow;
        return new DeadLetterEvent
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            SubscriptionId = subscriptionId,
            ErrorMessage = errorMessage,
            AttemptCount = attemptCount,
            FirstAttemptAt = now,
            LastAttemptAt = now,
            IsResolved = false
        };
    }

    public void MarkResolved()
    {
        IsResolved = true;
        ResolvedAt = DateTimeOffset.UtcNow;
    }
}
