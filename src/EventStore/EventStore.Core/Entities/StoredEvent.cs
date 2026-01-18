namespace EventStore.Core.Entities;

/// <summary>
/// Represents a persisted domain event in the event store.
/// </summary>
public sealed class StoredEvent
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public long SequenceNumber { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public Guid? CausationId { get; private set; }
    public int Version { get; private set; }
    public string Data { get; private set; } = string.Empty;
    public string? Metadata { get; private set; }
    public string? Hash { get; private set; }

    private StoredEvent() { } // For EF Core

    public static StoredEvent Create(
        string eventType,
        Guid aggregateId,
        string aggregateType,
        long sequenceNumber,
        string data,
        Guid? userId = null,
        Guid? correlationId = null,
        Guid? causationId = null,
        int version = 1,
        string? metadata = null)
    {
        return new StoredEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            SequenceNumber = sequenceNumber,
            Timestamp = DateTimeOffset.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId,
            Version = version,
            Data = data,
            Metadata = metadata
        };
    }

    public void SetHash(string hash)
    {
        Hash = hash;
    }
}
