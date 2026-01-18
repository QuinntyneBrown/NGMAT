using MessagePack;

namespace Shared.Messaging.UdpMulticast;

/// <summary>
/// Internal message format for UDP multicast transport.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
internal sealed class UdpMessage
{
    /// <summary>
    /// Unique message ID for deduplication.
    /// </summary>
    [Key(0)]
    public Guid MessageId { get; set; }

    /// <summary>
    /// Channel name for routing.
    /// </summary>
    [Key(1)]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Serialized event envelope payload.
    /// </summary>
    [Key(2)]
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Timestamp when the message was sent.
    /// </summary>
    [Key(3)]
    public long TimestampTicks { get; set; } = DateTimeOffset.UtcNow.Ticks;
}
