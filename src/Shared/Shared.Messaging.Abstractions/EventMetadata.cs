using MessagePack;

namespace Shared.Messaging.Abstractions;

/// <summary>
/// Metadata associated with an event for tracing and auditing.
/// </summary>
[MessagePackObject]
public sealed class EventMetadata
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    [Key(0)]
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    [Key(1)]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Correlation ID for request tracing across services.
    /// </summary>
    [Key(2)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// ID of the event that caused this event.
    /// </summary>
    [Key(3)]
    public Guid? CausationId { get; set; }

    /// <summary>
    /// ID of the user who triggered this event.
    /// </summary>
    [Key(4)]
    public string? UserId { get; set; }

    /// <summary>
    /// Name of the source service that published this event.
    /// </summary>
    [Key(5)]
    public string? SourceService { get; set; }

    /// <summary>
    /// Schema version for backward compatibility.
    /// </summary>
    [Key(6)]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Event type name for deserialization.
    /// </summary>
    [Key(7)]
    public string? EventType { get; set; }

    /// <summary>
    /// Additional custom properties.
    /// </summary>
    [Key(8)]
    public Dictionary<string, string>? Properties { get; set; }

    /// <summary>
    /// Creates metadata from an event.
    /// </summary>
    public static EventMetadata FromEvent(IEvent @event)
    {
        return new EventMetadata
        {
            EventId = @event.EventId,
            Timestamp = @event.Timestamp,
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            UserId = @event.UserId,
            SourceService = @event.SourceService,
            Version = @event.Version,
            EventType = @event.GetType().FullName
        };
    }
}
