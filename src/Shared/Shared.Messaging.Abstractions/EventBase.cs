namespace Shared.Messaging.Abstractions;

/// <summary>
/// Abstract base class for all events providing common functionality.
/// Derived classes should apply [MessagePackObject] and appropriate [Key] attributes.
/// </summary>
public abstract class EventBase : IEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    public Guid? CausationId { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }

    /// <inheritdoc />
    public int Version { get; init; } = 1;

    /// <inheritdoc />
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
