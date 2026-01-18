namespace EventStore.Core.Entities;

/// <summary>
/// Represents a subscription to specific event types.
/// </summary>
public sealed class Subscription
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string EventTypes { get; private set; } = string.Empty; // Comma-separated list
    public string? AggregateTypes { get; private set; } // Optional filter by aggregate type
    public string CallbackUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastDeliveryAt { get; private set; }
    public long LastDeliveredSequence { get; private set; }
    public int FailureCount { get; private set; }
    public DateTimeOffset? NextRetryAt { get; private set; }

    private Subscription() { } // For EF Core

    public static Subscription Create(
        string name,
        IEnumerable<string> eventTypes,
        string callbackUrl,
        IEnumerable<string>? aggregateTypes = null)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            Name = name,
            EventTypes = string.Join(",", eventTypes),
            AggregateTypes = aggregateTypes != null ? string.Join(",", aggregateTypes) : null,
            CallbackUrl = callbackUrl,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastDeliveredSequence = 0,
            FailureCount = 0
        };
    }

    public IEnumerable<string> GetEventTypes() => EventTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);
    public IEnumerable<string>? GetAggregateTypes() => AggregateTypes?.Split(',', StringSplitOptions.RemoveEmptyEntries);

    public void RecordDelivery(long sequenceNumber)
    {
        LastDeliveryAt = DateTimeOffset.UtcNow;
        LastDeliveredSequence = sequenceNumber;
        FailureCount = 0;
        NextRetryAt = null;
    }

    public void RecordFailure(TimeSpan backoffDelay)
    {
        FailureCount++;
        NextRetryAt = DateTimeOffset.UtcNow.Add(backoffDelay);
    }

    public void Activate()
    {
        IsActive = true;
        FailureCount = 0;
        NextRetryAt = null;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
