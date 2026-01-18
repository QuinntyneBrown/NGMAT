using MessagePack;

namespace Shared.Messaging.Abstractions;

/// <summary>
/// Abstract base class for all events providing common functionality.
/// Derived classes should apply [MessagePackObject] and use [Key] starting from 10.
/// Keys 0-9 are reserved for base class properties.
/// </summary>
public abstract class EventBase : IEvent
{
    /// <inheritdoc />
    [Key(0)]
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    [Key(1)]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    [Key(2)]
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    [Key(3)]
    public Guid? CausationId { get; init; }

    /// <inheritdoc />
    [Key(4)]
    public string? UserId { get; init; }

    /// <inheritdoc />
    [Key(5)]
    public int Version { get; init; } = 1;

    /// <inheritdoc />
    [Key(6)]
    public string? SourceService { get; init; }

    /// <summary>
    /// Creates a new event with generated ID and current timestamp.
    /// </summary>
    protected EventBase()
    {
    }

    /// <summary>
    /// Creates a new event caused by another event, inheriting correlation context.
    /// </summary>
    /// <param name="causedBy">The event that caused this event.</param>
    protected EventBase(IEvent causedBy)
    {
        CorrelationId = causedBy.CorrelationId;
        CausationId = causedBy.EventId;
        UserId = causedBy.UserId;
    }

    /// <summary>
    /// Gets the event metadata.
    /// </summary>
    public EventMetadata GetMetadata() => EventMetadata.FromEvent(this);
}
