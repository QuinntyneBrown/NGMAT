namespace Shared.Messaging.Abstractions;

/// <summary>
/// Base interface for all events in the system.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Correlation ID for request tracing across services.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// ID of the event that caused this event (for event chaining).
    /// </summary>
    Guid? CausationId { get; }

    /// <summary>
    /// ID of the user who triggered this event.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Schema version for backward compatibility.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Name of the source service that published this event.
    /// </summary>
    string? SourceService { get; }
}
