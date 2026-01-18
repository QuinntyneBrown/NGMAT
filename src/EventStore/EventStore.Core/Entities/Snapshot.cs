namespace EventStore.Core.Entities;

/// <summary>
/// Represents a snapshot of aggregate state for optimized replay.
/// </summary>
public sealed class Snapshot
{
    public Guid Id { get; private set; }
    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public long SequenceNumber { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string Data { get; private set; } = string.Empty;
    public int Version { get; private set; }

    private Snapshot() { } // For EF Core

    public static Snapshot Create(
        Guid aggregateId,
        string aggregateType,
        long sequenceNumber,
        string data,
        int version = 1)
    {
        return new Snapshot
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            SequenceNumber = sequenceNumber,
            Timestamp = DateTimeOffset.UtcNow,
            Data = data,
            Version = version
        };
    }
}
