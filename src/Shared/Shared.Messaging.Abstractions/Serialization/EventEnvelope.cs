using MessagePack;

namespace Shared.Messaging.Abstractions.Serialization;

/// <summary>
/// Envelope for transporting events with metadata and type information.
/// </summary>
[MessagePackObject]
public sealed class EventEnvelope
{
    /// <summary>
    /// The event metadata.
    /// </summary>
    [Key(0)]
    public EventMetadata Metadata { get; set; } = new();

    /// <summary>
    /// The serialized event payload.
    /// </summary>
    [Key(1)]
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Creates an envelope for an event.
    /// </summary>
    public static EventEnvelope Create<TEvent>(TEvent @event, IMessageSerializer serializer)
        where TEvent : class, IEvent
    {
        var metadata = EventMetadata.FromEvent(@event);
        metadata.EventType = typeof(TEvent).AssemblyQualifiedName;

        return new EventEnvelope
        {
            Metadata = metadata,
            Payload = serializer.Serialize(@event)
        };
    }

    /// <summary>
    /// Extracts the event from the envelope.
    /// </summary>
    public TEvent? GetEvent<TEvent>(IMessageSerializer serializer)
        where TEvent : class, IEvent
    {
        return serializer.Deserialize<TEvent>(Payload);
    }

    /// <summary>
    /// Extracts the event from the envelope using the type from metadata.
    /// </summary>
    public IEvent? GetEvent(IMessageSerializer serializer)
    {
        if (string.IsNullOrEmpty(Metadata.EventType))
        {
            throw new InvalidOperationException("Event type not specified in metadata");
        }

        var type = Type.GetType(Metadata.EventType);
        if (type == null)
        {
            throw new InvalidOperationException($"Could not resolve event type: {Metadata.EventType}");
        }

        return serializer.Deserialize(Payload, type) as IEvent;
    }
}
